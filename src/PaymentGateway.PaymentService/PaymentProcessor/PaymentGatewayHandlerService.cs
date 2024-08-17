using System.Diagnostics;

using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Infrastructure;

using Serilog;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public class PaymentGatewayHandlerService : BackgroundService
    {
        private readonly QueueClient _queueClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly EventHubSimulatorService _eventHubSimulatorService;
        private readonly PaymentQueueService _paymentQueueService;
        private readonly IEnumerable<IHandlePaymentGateway> _handlePaymentGateway;
        private readonly ILogger _logger;

        public PaymentGatewayHandlerService(
            QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            EventHubSimulatorService eventHubSimulatorService,
            PaymentQueueService paymentQueueService,
            IEnumerable<IHandlePaymentGateway> handlePaymentGateway,
            ILogger logger)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _paymentRepository = paymentRepository;
            _eventHubSimulatorService = eventHubSimulatorService;
            _paymentQueueService = paymentQueueService;
            _handlePaymentGateway = handlePaymentGateway;
            _logger = logger;
        }
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.Information("PaymentInitializerProcessor is starting.");
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _queueClient.ReceiveMessageAsync(cancellationToken: cancellationToken);

                if (message.Value == null)
                {
                    continue;
                }

                _logger.Information("Reciveved message: {Message}", message.Value.MessageText);
                var payment = JsonConvert.DeserializeObject<PaymentDto>(message.Value.MessageText);

                if (payment != null)
                {
                    switch ((PaymentStatus)Enum.Parse(typeof(PaymentStatus), payment.Status))
                    {
                        case PaymentStatus.Creating:
                            await _handlePaymentGateway.OfType<CreatePaymentProcessor>()
                                .FirstOrDefault()!
                                .HandlePyament(payment, cancellationToken);
                            break;
                        default:

                        case PaymentStatus.Processing:
                            await _handlePaymentGateway!.OfType<ProcessPaymentsHandler>()
                            .FirstOrDefault()!
                            .HandlePyament(payment, cancellationToken);
                            break;
                    }
                }

                await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
            }
        }
    }

}