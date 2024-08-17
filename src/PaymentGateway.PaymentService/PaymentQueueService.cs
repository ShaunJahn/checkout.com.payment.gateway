using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Serilog;
using Newtonsoft.Json;
using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.PaymentService
{
    public class PaymentQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger _logger;

        public PaymentQueueService(
            QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            ILogger logger)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
            _logger = logger;
        }

        public async Task SendPaymentAsync(PaymentDto payment, CancellationToken cancellationToken)
        {
            var message = JsonConvert.SerializeObject(payment);
            _logger.Information("Sending Payment To Queue To Be Processed: {PaymentId}", payment.id);
            await _queueClient.SendMessageAsync(message, cancellationToken: cancellationToken);
        }
    }
}
