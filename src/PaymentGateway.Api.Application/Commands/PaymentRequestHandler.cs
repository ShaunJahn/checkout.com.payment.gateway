using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.PaymentService;

using Serilog;

namespace PaymentGateway.Api.Application.Commands
{
    public class PaymentRequestHandler(
        PaymentQueueService paymentQueueService,
        ILogger logger,
        EventHubSimulatorService eventHubSimulatorService) : ICommandHandler<PaymentRequestCommand, PaymentRequestResponse>
    {
        private readonly PaymentQueueService _paymentQueueService = paymentQueueService;
        private readonly EventHubSimulatorService _eventHubSimulatorService = eventHubSimulatorService;
        private readonly ILogger _logger = logger;

        public async Task<PaymentRequestResponse> Handle(
            PaymentRequestCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Received Payment Request From Client Card Ending In: {CardEndingFourDigits}", request.CardNumber.Substring(request.CardNumber.Length - 4));

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

            await _paymentQueueService.SendPaymentAsync(payment, cancellationToken);
            await _eventHubSimulatorService.SendMessageAsync(
               message: new PaymentEvent(payment.id,
                                         payment.Timestamp,
                                         payment.Status,
                                         payment.CardNumber,
                                         payment.ExpiryMonth,
                                         payment.ExpiryYear,
                                         payment.Currency,
                                         payment.Amount),
               payment.id, cancellationToken);

            _logger.Information("Payment Successfully Created And Will Be Processed: {PaymentRequest}", payment.id);

            return new PaymentRequestResponse
            {
                Status = payment.Status,
                TimeStamp = payment.Timestamp,
                PaymentId = payment.id
            };
        }
    }
}
