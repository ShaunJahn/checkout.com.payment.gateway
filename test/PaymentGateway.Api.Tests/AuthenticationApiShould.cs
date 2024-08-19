using FluentAssertions;

using Xunit;

namespace PaymentGateway.Api.Tests
{
    public class AuthenticationApiShould : IClassFixture<IntegrationStartUp<Program>>
    {
        private readonly HttpClient _client;
        private readonly AuthHelper _authHelper;

        public AuthenticationApiShould(IntegrationStartUp<Program> factory)
        {
            _client = factory.CreateClient();
            _authHelper = new AuthHelper(_client);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsToken_WhenValidRequest()
        {
            var token = await _authHelper.GetAuthTokenAsync();
            token.Should().NotBeNullOrEmpty();
        }
    }
}