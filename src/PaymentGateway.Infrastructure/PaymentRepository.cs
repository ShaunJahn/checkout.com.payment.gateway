using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

using PaymentGateway.Contracts.V1.Dtos;

using Serilog;

namespace PaymentGateway.Infrastructure
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly Container _container;
        private readonly ILogger _logger;

        public PaymentRepository(
            CosmosClient cosmosClient,
            IConfiguration configuration,
            ILogger logger)
        {
            string databaseId = configuration["CosmosDb:DatabaseId"]
                ?? throw new ArgumentNullException("CosmosDb:DatabaseId", "The CosmosDb:DatabaseId configuration setting is missing or null.");
            string containerId = configuration["CosmosDb:ContainerId"]
                ?? throw new ArgumentNullException("CosmosDb:ContainerId", "The CosmosDb:ContainerId configuration setting is missing or null.");

            _container = cosmosClient.GetContainer(databaseId, containerId);
            _logger = logger;
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(string id)
        {
            try
            {
                ItemResponse<PaymentDto> response = await _container.ReadItemAsync<PaymentDto>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new PaymentDto();
            }
        }

        public async Task UpdatePaymentStatusAsync(string id, string status)
        {
            var payment = await GetPaymentByIdAsync(id);
            if (payment != null)
            {
                payment.Status = status;
                payment.Timestamp = DateTime.UtcNow;
                await UpsertPaymentAsync(payment);
            }
        }

        public async Task<PaymentDto> UpsertPaymentAsync(PaymentDto payment)
        {
            try
            {
                var response = await _container.CreateItemAsync(payment, new PartitionKey(payment.id));
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.Error($"Cosmos DB error: {ex.StatusCode} - {ex.Message}");
                _logger.Error($"Diagnostics: {ex.Diagnostics}");
                throw;
            }
        }

    }
}
