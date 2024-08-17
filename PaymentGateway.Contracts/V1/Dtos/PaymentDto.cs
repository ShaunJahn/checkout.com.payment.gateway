namespace PaymentGateway.Contracts.V1.Dtos
{
    public class PaymentDto
    {
        public string id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public string Cvv { get; set; }
    }
}
