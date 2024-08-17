namespace PaymentGateway.Api.Application.Commands
{
    public record PaymentRequestCommand : ICommand<PaymentRequestResponse>
    {
        public required string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public required string Currency { get; set; }
        public int Amount { get; set; }
        public required string Cvv { get; set; }
    }

    public record PaymentRequestResponse
    {
        public required string PaymentId { get; set; }
        public DateTime TimeStamp { get; set; }
        public required string Status { get; set; }
    }
}
