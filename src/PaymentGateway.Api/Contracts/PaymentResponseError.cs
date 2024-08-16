namespace PaymentGateway.Api.Contracts
{
    public class PaymentResponse
    {
        public Guid TransactionId { get; set; }
        public DateTime TimeStamp { get; set; }
        public required string Status { get; set; }
    }
}
