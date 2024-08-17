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
                throw new Exception("TEMP");
            }
        }

        public async Task UpdatePaymentStatusAsync(PaymentDto paymentUpdate)
        {
            var payment = await GetPaymentByIdAsync(paymentUpdate.id);
            if (payment != null)
            {
                payment.Status = paymentUpdate.Status;
                payment.AuthorizationCode = payment.AuthorizationCode;
                await _container.UpsertItemAsync(payment);
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
