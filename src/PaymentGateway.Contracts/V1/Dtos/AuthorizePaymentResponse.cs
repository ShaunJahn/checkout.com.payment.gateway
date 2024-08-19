using Newtonsoft.Json;

namespace PaymentGateway.Contracts.V1.Dtos
{
    public class AuthorizePaymentResponse
    {
        [JsonProperty("authorized")]
        public bool IsAuthorized { get; set; }
        [JsonProperty("authorization_code")]
        public Guid? AuthorizationCode { get; set; }
    }
}
