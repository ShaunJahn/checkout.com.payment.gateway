using System.Text;

using Newtonsoft.Json;

namespace PaymentGateway.Api.Tests
{
    public class AuthHelper
    {
        private readonly HttpClient _client;

        public AuthHelper(HttpClient client)
        {
            _client = client;
        }
        public async Task<string> GetAuthTokenAsync()
        {
            var authRequest = new
            {
                Username = "DemoClient",
                Password = "SomePassword"
            };

            var jsonContent = JsonConvert.SerializeObject(authRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/auth/authenticate", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }
    }
}
