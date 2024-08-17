namespace PaymentGateway.Contracts.V1.Dtos
{
    public class PaymentDto
    {
        public required string id { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Status { get; set; }
        public required string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public required string Currency { get; set; }
        public int Amount { get; set; }
        public required string Cvv { get; set; }
        public Guid AuthorizationCode { get; set; }
    }
}
