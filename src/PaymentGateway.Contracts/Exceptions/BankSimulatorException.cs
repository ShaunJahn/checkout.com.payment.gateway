namespace PaymentGateway.Contracts.Exceptions
{
    public class BankSimulatorException : Exception
    {
        public DateTime Timestamp { get; private set; }
        public string CustomMessage { get; private set; }
        public BankSimulatorException(string message) : base(message)
        {
            Timestamp = DateTime.UtcNow;
            CustomMessage = message;
        }
    }
}
