using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class PaymentCreatedResponseExample : IExamplesProvider<PaymentCreatedResponse>
    {
        public PaymentCreatedResponse GetExamples()
        {
            return new PaymentCreatedResponse()
            {
                Status = "Created",
                TimeStamp = DateTime.Now,
                PaymentId = Guid.NewGuid()
            };
        }
    }
}
