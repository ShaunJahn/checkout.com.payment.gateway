using System.Threading;

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

        public async Task<PaymentDto> GetPaymentByIdAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                ItemResponse<PaymentDto> response = await _container.ReadItemAsync<PaymentDto>(id, new PartitionKey(id), cancellationToken: cancellationToken);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("TEMP");
            }
        }

        public async Task UpdatePaymentStatusAsync(PaymentDto paymentUpdate, CancellationToken cancellationToken)
        {
            var payment = await GetPaymentByIdAsync(paymentUpdate.id, cancellationToken: cancellationToken);
            if (payment != null)
            {
                payment.Status = paymentUpdate.Status;
                payment.AuthorizationCode = paymentUpdate.AuthorizationCode;
                await _container.UpsertItemAsync(payment, cancellationToken: cancellationToken);
            }
        }

        public async Task<PaymentDto> UpsertPaymentAsync(PaymentDto payment, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _container.UpsertItemAsync(payment, new PartitionKey(payment.id), cancellationToken: cancellationToken);
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
