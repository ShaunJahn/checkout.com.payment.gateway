using System.Diagnostics;

using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Infrastructure;

using Serilog;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public class CreatePaymentProcessor : IHandlePaymentGateway
    {
        private readonly QueueClient _queueClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger _logger;
        private readonly EventHubSimulatorService _eventHubSimulatorService;
        private readonly PaymentQueueService _paymentQueueService;

        public CreatePaymentProcessor(
            QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            ILogger logger,
            EventHubSimulatorService eventHubSimulatorService,
            PaymentQueueService paymentQueueService)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _paymentRepository = paymentRepository;
            _logger = logger;
            _eventHubSimulatorService = eventHubSimulatorService;
            _paymentQueueService = paymentQueueService;
        }
        public async Task HandlePyament(PaymentDto payment, CancellationToken stoppingToken)
        {
            _logger.Information("Creating payment: {Payment}", payment);

            payment.Status = PaymentStatus.Processing.ToString();

            await _paymentRepository.UpsertPaymentAsync(payment);
            await _eventHubSimulatorService.SendMessageAsync(payment);
            await _paymentQueueService.SendPaymentAsync(payment);

            _logger.Information("Payment Sucessfully created with PaymentId: {PaymentId} and Status: {Status}", payment.id, payment.Status);
        }
    }
}
