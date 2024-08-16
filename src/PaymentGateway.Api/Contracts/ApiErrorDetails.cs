namespace PaymentGateway.Api.Contracts
{
    public class ApiErrorDetails
    {
        public required Dictionary<string, string> MetaData { get; set; }
    }
}
