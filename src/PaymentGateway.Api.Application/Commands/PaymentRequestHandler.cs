using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.PaymentService;

using Serilog;

namespace PaymentGateway.Api.Application.Commands
{
    public class PaymentRequestHandler(PaymentQueueService paymentQueueService, ILogger logger, EventHubSimulatorService eventHubSimulatorService) : ICommandHandler<PaymentRequestCommand, PaymentRequestResponse>
    {
        private readonly PaymentQueueService _paymentQueueService = paymentQueueService;
        private readonly ILogger _logger = logger;
        private readonly EventHubSimulatorService _eventHubSimulatorService = eventHubSimulatorService;

        public async Task<PaymentRequestResponse> Handle(PaymentRequestCommand request, CancellationToken cancellationToken)
        {
            _logger.Information("Received Payment Request: {PaymentRequest}", request);
            var payment = new PaymentDto
            {
                id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Status = PaymentStatus.Creating.ToString(),
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
                Cvv = request.Cvv
            };

            await _paymentQueueService.SendPaymentAsync(payment);
            await _eventHubSimulatorService.SendMessageAsync(payment);

            _logger.Information("Payment Request Sent: {PaymentRequest}", payment);

            return new PaymentRequestResponse
            {
                Status = payment.Status,
                TimeStamp = payment.Timestamp,
                TransactionId = payment.id
            };
        }
    }
}
