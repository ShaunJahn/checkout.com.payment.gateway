

using MediatR;

using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Infrastructure;

using Serilog;

namespace PaymentGateway.Api.Application.Commands
{
    public class RetrievePaymentHandler(
        IPaymentRepository paymentRepository,
        ILogger logger) : IQueryHandler<RetrievePaymentQuery, RetrievePaymentQueryResponse>
    {
        private readonly ILogger _logger = logger;
        private readonly IPaymentRepository _paymentRepository = paymentRepository;

        async Task<RetrievePaymentQueryResponse> IRequestHandler<RetrievePaymentQuery, RetrievePaymentQueryResponse>.Handle(RetrievePaymentQuery request, CancellationToken cancellationToken)
        {
            var paymentDto = await _paymentRepository.GetPaymentByIdAsync(request.Id, cancellationToken);

            return new RetrievePaymentQueryResponse
            {
                Id = paymentDto.id,
                Status = paymentDto.Status,
                LastFourCardDigits = paymentDto.CardNumber.Substring(paymentDto.CardNumber.Length - 4),
                ExpiryMonth = paymentDto.ExpiryMonth,
                ExpiryYear = paymentDto.ExpiryYear,
                Currency = paymentDto.Currency,
                Amount = paymentDto.Amount
            };
        }
    }
}
