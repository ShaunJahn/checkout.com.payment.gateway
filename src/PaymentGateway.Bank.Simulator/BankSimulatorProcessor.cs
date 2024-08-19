using Newtonsoft.Json;

using PaymentGateway.Contracts.Exceptions;
using PaymentGateway.Contracts.V1.Dtos;

using Serilog;
namespace PaymentGateway.Bank.Simulator
{
    public class BankSimulatorProcessor : IBankSimulatorProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private const string PaymentUrl = "payments";

        public BankSimulatorProcessor(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(BankSimulatorProcessor));
            _logger = logger;
        }

        public async Task<AuthorizePaymentResponse> AuthorizePaymentAsync(PaymentRequest paymentRequest, string id, CancellationToken cancellationToken)
        {
            _logger.Information("Sending payment to acquiring bank: {PaymentId}", id);
            try
            {
                var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(PaymentUrl, content, cancellationToken);

                _logger.Information("Payment response from acquiring bank: {PaymentId} - {StatusCode}", id, response.StatusCode);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authorizePaymentResponse = JsonConvert.DeserializeObject<AuthorizePaymentResponse>(jsonResponse);
                _logger.Information("Payment authorized: {PaymentId} - {IsAuthorized}", id, authorizePaymentResponse.IsAuthorized);

                return authorizePaymentResponse;
            }
            catch (Exception ex)
            {
                throw new BankSimulatorException($"Error authorizing payment due to: {ex.Message}");
            }
        }
    }
}
