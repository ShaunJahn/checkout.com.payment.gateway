using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PaymentGateway.Api.Tests
{
    public class IntegrationStartUp<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder.ConfigureAppConfiguration((context, configBuilder) =>
            //{
            //    var projectDir = Directory.GetCurrentDirectory();
            //    var configPath = Path.Combine(projectDir, "IntegrationAppSettings.json");
            //    configBuilder.AddJsonFile(configPath);
            //});
            base.ConfigureWebHost(builder);
        }
    }
}