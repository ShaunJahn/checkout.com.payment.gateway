using System.Net.Http.Headers;
using System.Text;

using FluentAssertions;

using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using Newtonsoft.Json;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Tests;
using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;

using Xunit;

public class PaymentGatewayApiShould : IClassFixture<IntegrationStartUp<Program>>
{
    private readonly HttpClient _client;
    private readonly AuthHelper _authHelper;

    public PaymentGatewayApiShould(IntegrationStartUp<Program> factory)
    {
        _client = factory.CreateClient();
        _authHelper = new AuthHelper(_client);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsAccepted_WhenValidRequest()
    {
        var token = await _authHelper.GetAuthTokenAsync();

        // Arrange
        var request = new ClientPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var jsonContent = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/payment", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PaymentCreatedResponse>(responseContent);
        result.Should().NotBeNull();
        result!.PaymentId.Should().NotBeEmpty();
        result!.TimeStamp.Should().HaveYear(DateTime.UtcNow.Year);
        result!.Status.Should().NotBeEmpty();
        result!.Status.Should().Be(PaymentStatus.Creating.ToString());
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsAccepted_WhenPaymentIsAuthorized()
    {
        var token = await _authHelper.GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ClientPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        var jsonContent = JsonConvert.SerializeObject(request);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/payment", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PaymentCreatedResponse>(responseContent);

        await CheckForStatusChange(result.PaymentId, request);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsAccepted_WhenPaymentIsNoyAuthorized()
    {
        var token = await _authHelper.GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ClientPaymentRequest
        {
            CardNumber = "2222405343248112",
            ExpiryMonth = 01,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 60000,
            Cvv = "456"
        };

        var jsonContent = JsonConvert.SerializeObject(request);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/payment", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PaymentCreatedResponse>(responseContent);

        await CheckForStatusChange(result.PaymentId, request);
    }

    private async Task CheckForStatusChange(Guid paymentId, ClientPaymentRequest request)
    {
        var maxRetries = 10;
        var delay = TimeSpan.FromSeconds(2);

        PaymentResponse resultGetResponse = null;

        for (int i = 0; i < maxRetries; i++)
        {
            var getResponse = await _client.GetAsync($"/payment?paymentId={paymentId}");
            if (getResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                resultGetResponse = JsonConvert.DeserializeObject<PaymentResponse>(responseContent)!;

                if (resultGetResponse!.Status == PaymentStatus.Authorized.ToString())
                {
                    break;
                }
            }
            else
                await Task.Delay(delay);
        }

        resultGetResponse.Should().NotBeNull();
        resultGetResponse.Status.Should().Be(PaymentStatus.Authorized.ToString());
        resultGetResponse.Id.Should().Be(paymentId.ToString());
        resultGetResponse.ExpiryMonth.Should().Be(request.ExpiryMonth);
        resultGetResponse.ExpiryYear.Should().Be(request.ExpiryYear);
        resultGetResponse.Currency.Should().Be(request.Currency);
        resultGetResponse.Amount.Should().Be(request.Amount);
        resultGetResponse.LastFourCardDigits.Should().Be(request.CardNumber.Substring(request.CardNumber.Length - 4));
    }
}