namespace PaymentGateway.Api.Contracts
{
    public class ValidationErrorResponse
    {
        public required string Title { get; set; }
        public int Status { get; set; }
        public required string Detail { get; set; }
        public required ErrorDetails Errors { get; set; }
    }

    public class ErrorDetails
    {
        public required List<ApiErrorDetail> ApiErrorDetailsList { get; set; }
    }

    public class ApiErrorDetail
    {
        public required MetaData MetaData { get; set; }
    }

    public class MetaData
    {
        public required string Property { get; set; }
        public required string ErrorMessage { get; set; }
    }

}
