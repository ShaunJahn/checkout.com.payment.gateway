using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class PaymentResponseExample : IExamplesProvider<PaymentResponse>
    {
        public PaymentResponse GetExamples()
        {
            return new PaymentResponse()
            {
                Status = "Created", TimeStamp = DateTime.Now, TransactionId = Guid.NewGuid()
            };
        }
    }
}
