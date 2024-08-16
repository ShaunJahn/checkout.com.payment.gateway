using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class RequestPaymentExample : IExamplesProvider<PaymentRequest>
    {
        public PaymentRequest GetExamples()
        {
            return new PaymentRequest
            {
                CardNumber = "12345678901232",
                Currency = "USD",
                Cvv = "334",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Amount = 100
            };
        }
    }
}
