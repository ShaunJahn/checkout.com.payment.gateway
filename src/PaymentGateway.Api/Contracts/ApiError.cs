namespace PaymentGateway.Api.Contracts
{
    public class ApiError
    {
        public required List<ApiErrorDetails> ApiErrorDetailsList { get; set; }
    }
}
