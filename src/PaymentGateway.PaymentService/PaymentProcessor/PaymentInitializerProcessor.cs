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
    public class PaymentInitializerProcessor : BackgroundService
    {
        private readonly QueueClient _queueClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger _logger;
        private readonly EventHubSimulatorService _eventHubSimulatorService;

        public PaymentInitializerProcessor(
            QueueServiceClient queueServiceClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            ILogger logger,
            EventHubSimulatorService eventHubSimulatorService)
        {
            var queueName = configuration["AzureStorage:QueueName"];
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _paymentRepository = paymentRepository;
            _logger = logger;
            _eventHubSimulatorService = eventHubSimulatorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("PaymentInitializerProcessor is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _queueClient.ReceiveMessageAsync(cancellationToken: stoppingToken);

                if (message.Value != null)
                {
                    _logger.Information("Processing message: {Message}", message.Value.MessageText);
                    var stopwatch = new Stopwatch();
                    var payment = JsonConvert.DeserializeObject<PaymentDto>(message.Value.MessageText);

                    _logger.Information("Creating payment: {Payment}", payment);

                    if (payment == null)
                    {
                        _logger.Error("Payment is null");
                        await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                        continue;
                    }

                    payment.Status = PaymentStatus.Processing.ToString();
                    
                    await _paymentRepository.UpsertPaymentAsync(payment);
                    await _eventHubSimulatorService.SendMessageAsync(payment);

                    _logger.Information("Payment Sucessfully created with PaymentId: {PaymentId} and Status: {Status} in {Stopwatch}ms", payment.id, payment.Status, stopwatch);
                    await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                }
            }
        }
    }

}