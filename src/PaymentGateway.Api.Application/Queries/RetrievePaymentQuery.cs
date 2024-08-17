namespace PaymentGateway.Api.Application.Queries
{
    public class RetrievePaymentQuery : IQuery<RetrievePaymentQueryResponse>
    {
        public required string Id { get; set; }
    }

    public record RetrievePaymentQueryResponse
    {
        public required string Id { get; set; }
        public required string Status { get; set; }
        public required string LastFourCardDigits { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public required string Currency { get; set; }
        public int Amount { get; set; }
    }
}