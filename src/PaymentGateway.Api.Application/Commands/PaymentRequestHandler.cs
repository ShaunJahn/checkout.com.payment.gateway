namespace PaymentGateway.Api.Application.Commands
{
    public class PaymentRequestHandler : ICommandHandler<PaymentRequestCommand, PaymentRequestResponse>
    {
        public PaymentRequestHandler()
        {

        }

        public async Task<PaymentRequestResponse> Handle(PaymentRequestCommand request, CancellationToken cancellationToken)
        {
            var response = new PaymentRequestResponse
            {
                TransactionId = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Status = "Success"
            };

            await Task.Delay(1, cancellationToken);

            return response;
        }
    }
}
