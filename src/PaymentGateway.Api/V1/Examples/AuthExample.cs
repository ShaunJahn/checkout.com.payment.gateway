using PaymentGateway.Api.Auth;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class AuthExample : IExamplesProvider<ClientCredentials>
    {
        public ClientCredentials GetExamples()
        {
            return new ClientCredentials() { Password = "SomePassword", UserName = "DemoClient" };
        }
    }
}
