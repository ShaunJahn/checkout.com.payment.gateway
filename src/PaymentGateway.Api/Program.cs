using System.Reflection;
using System.Text;

using Azure.Storage.Queues;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using PaymentGateway.Api;
using PaymentGateway.Api.Auth;
using PaymentGateway.Api.MiddleWare;
using PaymentGateway.Bank.Simulator;
using PaymentGateway.Infrastructure;
using PaymentGateway.PaymentService;
using PaymentGateway.PaymentService.PaymentProcessor;

using Polly;

using Serilog;

using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

SetLoggingContext(builder);
SetSwagger(builder);
SetMediatorContext(builder);
SetAuthenticationContext(builder);
SetCosmosContext(builder);

builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AzureStorage:ConnectionString"];
    return new QueueServiceClient(connectionString);
});

builder.Services.AddHostedService<PaymentGatewayHandlerService>();
builder.Services.AddHostedService<EventHubListenerService>();

builder.Services.AddSingleton<PaymentQueueService>();
builder.Services.AddSingleton<EventHubSimulatorService>();
builder.Services.AddTransient<IHandlePaymentGateway, ProcessPaymentsHandler>();
builder.Services.AddTransient<IHandlePaymentGateway, CreatePaymentProcessor>();
builder.Services.AddTransient<IBankSimulatorProcessor, BankSimulatorProcessor>();

var bankSimulatorConfig = builder.Configuration.GetSection("BankSimulator");
builder.Services.AddHttpClient<BankSimulatorProcessor>(client =>
{
    client.BaseAddress = new Uri(bankSimulatorConfig["BaseUrl"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Run();

static void SetLoggingContext(WebApplicationBuilder builder)
{
    Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(builder.Configuration)
         .CreateLogger(); ;

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog(Log.Logger);
}

static void SetSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Checkout.com Payment API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
        c.ExampleFilters();
    });
}

static void SetMediatorContext(WebApplicationBuilder builder)
{
    builder.Services.AddMediator();
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();
}

static void SetCosmosContext(WebApplicationBuilder builder)
{
    builder.Services.Configure<CosmosDbSettings>(options => builder.Configuration.GetSection("CosmosDb").Bind(options));
    builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;

        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }),
            ConnectionMode = ConnectionMode.Gateway
        };

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetry(
                retryCount: int.MaxValue,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine("Waiting for Cosmos database to start...");
                    Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Retrying in {timeSpan}.");
                });

        var cosmosClient = new CosmosClient(settings.Account, settings.Key, options);

        retryPolicy.Execute(() =>
        {
            var database = cosmosClient.CreateDatabaseIfNotExistsAsync(settings.DatabaseId).GetAwaiter().GetResult();
            database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = settings.ContainerId,
                PartitionKeyPath = "/id"
            }).GetAwaiter().GetResult();
        });

        return cosmosClient;
    });
}

static void SetAuthenticationContext(WebApplicationBuilder builder)
{
    builder.Services.AddTransient<IJwtTokenManager, JwtTokenManager>();
    builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwtOption =>
    {
        var key = "SomeKeyVaultKeyOrAppSettingsKeyHardCodedForDEMO!]]]]";
        var keyBytes = Encoding.ASCII.GetBytes(key);
        jwtOption.SaveToken = true;
        jwtOption.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });
}