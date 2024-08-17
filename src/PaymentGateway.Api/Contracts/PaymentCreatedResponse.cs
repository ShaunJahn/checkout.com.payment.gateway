namespace PaymentGateway.Api.Contracts
{
    public class PaymentCreatedResponse
    {
        public Guid PaymentId { get; set; }
        public DateTime TimeStamp { get; set; }
        public required string Status { get; set; }
    }
}
