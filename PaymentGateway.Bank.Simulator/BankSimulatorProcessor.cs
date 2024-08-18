using Newtonsoft.Json;

using PaymentGateway.Contracts.Exceptions;
using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.Bank.Simulator
{
    public class BankSimulatorProcessor : IBankSimulatorProcessor
    {
        private readonly HttpClient _httpClient;
        private const string PaymentUrl = "payments";

        public BankSimulatorProcessor(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(BankSimulatorProcessor));
        }

        public async Task<AuthorizePaymentResponse> AuthorizePaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            try
            {
                var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(PaymentUrl, content, cancellationToken);

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authorizePaymentResponse = JsonConvert.DeserializeObject<AuthorizePaymentResponse>(jsonResponse);

                return authorizePaymentResponse;
            }
            catch (Exception ex)
            {
                throw new BankSimulatorException($"Error authorizing payment due to: {ex.Message}");
            }
        }
    }
}
