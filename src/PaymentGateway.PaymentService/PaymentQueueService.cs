using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.PaymentService
{
    public class PaymentQueueService
    {
        private readonly QueueClient _queueClient;

        public PaymentQueueService(QueueServiceClient queueServiceClient, IConfiguration configuration)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendPaymentAsync(PaymentDto payment)
        {
            var message = JsonConvert.SerializeObject(payment);
            await _queueClient.SendMessageAsync(message);
        }
    }
}
