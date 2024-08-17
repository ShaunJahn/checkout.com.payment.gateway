using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

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
        private readonly EventHubSimulatorService _eventHubSimulatorService;
        private readonly PaymentQueueService _paymentQueueService;
        private readonly ILogger _logger;

        public CreatePaymentProcessor(
            QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            EventHubSimulatorService eventHubSimulatorService,
            PaymentQueueService paymentQueueService,
            ILogger logger)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _paymentRepository = paymentRepository;
            _eventHubSimulatorService = eventHubSimulatorService;
            _paymentQueueService = paymentQueueService;
            _logger = logger;
        }
        public async Task HandlePyament(PaymentDto payment, CancellationToken cancellationToken)
        {
            _logger.Information("Creating payment: {Payment}", payment);

            payment.Status = PaymentStatus.Processing.ToString();

            await _paymentRepository.UpsertPaymentAsync(payment, cancellationToken);
            await _eventHubSimulatorService.SendMessageAsync(payment, payment.id, cancellationToken);
            await _paymentQueueService.SendPaymentAsync(payment, cancellationToken);

            _logger.Information("Payment Sucessfully created with PaymentId: {PaymentId} and Status: {Status}", payment.id, payment.Status);
        }
    }
}
