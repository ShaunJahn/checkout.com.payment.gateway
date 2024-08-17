using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Api.Application.Commands
{
    public class PaymentRequestHandler(IPaymentRepository paymentRepository) : ICommandHandler<PaymentRequestCommand, PaymentRequestResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;

        public async Task<PaymentRequestResponse> Handle(PaymentRequestCommand request, CancellationToken cancellationToken)
        {
            var payment = new PaymentDto
            {
                id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Status = "Pending",
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
                Cvv = request.Cvv
            };

            var response = await _paymentRepository.UpsertPaymentAsync(payment);

            return new PaymentRequestResponse
            {
                Status = response.Status,
                TimeStamp = response.Timestamp,
                TransactionId = Guid.Parse(response.id)
            };
        }
    }
}
