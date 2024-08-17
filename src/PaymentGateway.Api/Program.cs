using System.Reflection;
using System.Text;

using Azure.Storage.Queues;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using PaymentGateway.Api;
using PaymentGateway.Api.Auth;
using PaymentGateway.Api.MiddleWare;
using PaymentGateway.Infrastructure;
using PaymentGateway.PaymentService;
using PaymentGateway.PaymentService.PaymentProcessor;

using Serilog;

using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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
});
builder.Services.AddSwaggerGen(options =>
{
    options.ExampleFilters();
});

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

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
builder.Services.AddMediator();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<IJwtTokenManager, JwtTokenManager>();

builder.Services.Configure<CosmosDbSettings>(options => builder.Configuration.GetSection("CosmosDb").Bind(options));
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
    var cosmosClient = new CosmosClient(settings.Account, settings.Key);

    var database = cosmosClient.CreateDatabaseIfNotExistsAsync(settings.DatabaseId).GetAwaiter().GetResult();
    database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
    {
        Id = settings.ContainerId,
        PartitionKeyPath = "/id"
    }).GetAwaiter().GetResult();

    return cosmosClient;
});
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();


builder.Services.AddSingleton<QueueServiceClient>(serviceProvider =>
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
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Run();
