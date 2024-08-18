namespace PaymentGateway.Contracts.Exceptions
{
    public class WebhookException : Exception
    {
        public DateTime Timestamp { get; private set; }
        public string CustomMessage { get; private set; }
        public WebhookException(string message) : base(message)
        {
            Timestamp = DateTime.UtcNow;
            CustomMessage = message;
        }
    }
}
