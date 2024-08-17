using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class PaymentResponseExample : IExamplesProvider<PaymentResponse>
    {
        public PaymentResponse GetExamples()
        {
            return new PaymentResponse
            {
                Id = Guid.NewGuid().ToString(),
                Status = "Authorized",
                LastFourCardDigits = "1234",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Currency = "USD",
                Amount = 100
            };
        }
    }
}
