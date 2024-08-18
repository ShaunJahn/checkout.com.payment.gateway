using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

using PaymentGateway.Bank.Simulator;
using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Infrastructure;

using Serilog;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public class ProcessPaymentsHandler : IHandlePaymentGateway
    {
        private readonly QueueClient _queueClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger _logger;
        private readonly EventHubSimulatorService _eventHubSimulatorService;
        private readonly PaymentQueueService _paymentQueueService;
        private readonly IBankSimulatorProcessor _bankSimulatorProcessor;

        public ProcessPaymentsHandler(
       QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            ILogger logger,
            EventHubSimulatorService eventHubSimulatorService,
            PaymentQueueService paymentQueueService,
            IBankSimulatorProcessor bankSimulatorProcessor)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _paymentRepository = paymentRepository;
            _logger = logger;
            _eventHubSimulatorService = eventHubSimulatorService;
            _paymentQueueService = paymentQueueService;
            _bankSimulatorProcessor = bankSimulatorProcessor;
        }
        public async Task HandlePyament(PaymentDto payment, CancellationToken cancellationToken)
        {
            _logger.Information("Sending payment to acquiring bank: {PaymentId}", payment.id);

            var response = await _bankSimulatorProcessor.AuthorizePaymentAsync(
                new PaymentRequest(
                    cardNumber: payment.CardNumber,
                    expiryMonth: payment.ExpiryMonth,
                    expiryYear: payment.ExpiryYear,
                    currency: payment.Currency,
                    amount: payment.Amount,
                    cvv: payment.Cvv),
                cancellationToken);

            if (response.IsAuthorized is true)
            {
                payment.Status = PaymentStatus.Authorized.ToString();
                payment.AuthorizationCode = response.AuthorizationCode;
            }
            else
            {
                payment.Status = PaymentStatus.Failed.ToString();
            }

            await _paymentRepository.UpdatePaymentStatusAsync(payment, cancellationToken);
            await _eventHubSimulatorService.SendMessageAsync(payment, payment.id, cancellationToken);

            _logger.Information("Payment created with PaymentId: {PaymentId} and Status: {Status}", payment.id, payment.Status);
        }
    }
}
