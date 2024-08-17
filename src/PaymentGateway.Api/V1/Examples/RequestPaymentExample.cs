using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class RequestPaymentExample : IExamplesProvider<ClientPaymentRequest>
    {
        public ClientPaymentRequest GetExamples()
        {
            return new ClientPaymentRequest
            {
                CardNumber = "2222405343248877",
                Currency = "GBP",
                Cvv = "123",
                ExpiryMonth = 04,
                ExpiryYear = 2025,
                Amount = 100
            };
        }
    }
}
