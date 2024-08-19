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
    public async Task ProcessPaymentAsync_ReturnsErrors_Api_Validation()
    {
        var token = await _authHelper.GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ClientPaymentRequest
        {
            CardNumber = "asd",
            ExpiryMonth = 2,
            ExpiryYear = 1111,
            Currency = "sd",
            Amount = -22,
            Cvv = "AA"
        };

        var jsonContent = JsonConvert.SerializeObject(request);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/payment", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var validationErrorResponse = JsonConvert.DeserializeObject<ValidationErrorResponse>(responseContent);

        // Assert the general structure of the response
        validationErrorResponse.Should().NotBeNull();
        validationErrorResponse.Title.Should().Be("Validation Error");
        validationErrorResponse.Status.Should().Be(422);
        validationErrorResponse.Detail.Should().Be("Validation errors occurred for type PaymentRequestCommand");

        // Assert the validation errors
        var errors = validationErrorResponse.Errors.ApiErrorDetailsList;

        errors.Should().Contain(e => e.MetaData.Property == "CardNumber" && e.MetaData.ErrorMessage == "Card number must be between 14 and 19 characters long");

        errors.Should().Contain(e => e.MetaData.Property == "CardNumber" && e.MetaData.ErrorMessage == "Card number must contain only numeric characters");

        errors.Should().Contain(e => e.MetaData.Property == "ExpiryYear" && e.MetaData.ErrorMessage == "Expiry year must be in the future");

        errors.Should().Contain(e => e.MetaData.Property == "Currency" && e.MetaData.ErrorMessage == "Currency code must be 3 characters long");

        errors.Should().Contain(e => e.MetaData.Property == "Currency" && e.MetaData.ErrorMessage == "Currency must be USD, EUR, or GBP");

        errors.Should().Contain(e => e.MetaData.Property == "Amount" && e.MetaData.ErrorMessage == "Amount must be greater than zero");

        errors.Should().Contain(e => e.MetaData.Property == "Cvv" && e.MetaData.ErrorMessage == "CVV must be 3 or 4 digits long");

        errors.Should().Contain(e => e.MetaData.Property == "Cvv" && e.MetaData.ErrorMessage == "CVV must contain only numeric characters and be 3 or 4 digits");
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

        await CheckForStatusChange(result.PaymentId, request, PaymentStatus.Authorized);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsAccepted_WhenPaymentIsNotAuthorized()
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

        await CheckForStatusChange(result.PaymentId, request, PaymentStatus.Declined);
    }

    private async Task CheckForStatusChange(Guid paymentId, ClientPaymentRequest request, PaymentStatus status)
    {
        var maxRetries = 50;
        var delay = TimeSpan.FromSeconds(2);

        PaymentResponse resultGetResponse = null;

        for (int i = 0; i < maxRetries; i++)
        {
            var getResponse = await _client.GetAsync($"/payment?paymentId={paymentId}");
            if (getResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                resultGetResponse = JsonConvert.DeserializeObject<PaymentResponse>(responseContent)!;

                if (resultGetResponse!.Status == status.ToString())
                {
                    break;
                }
            }
            else
                await Task.Delay(delay);
        }

        resultGetResponse.Should().NotBeNull();
        resultGetResponse.Status.Should().Be(status.ToString());
        resultGetResponse.Id.Should().Be(paymentId.ToString());
        resultGetResponse.ExpiryMonth.Should().Be(request.ExpiryMonth);
        resultGetResponse.ExpiryYear.Should().Be(request.ExpiryYear);
        resultGetResponse.Currency.Should().Be(request.Currency);
        resultGetResponse.Amount.Should().Be(request.Amount);
        resultGetResponse.LastFourCardDigits.Should().Be(request.CardNumber.Substring(request.CardNumber.Length - 4));
    }
}