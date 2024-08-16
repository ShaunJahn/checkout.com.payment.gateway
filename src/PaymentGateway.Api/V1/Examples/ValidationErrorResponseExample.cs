using PaymentGateway.Api.Contracts;

using Swashbuckle.AspNetCore.Filters;

namespace PaymentGateway.Api.V1.Examples
{
    public class ValidationErrorResponseExample : IExamplesProvider<ValidationErrorResponse>
    {
        public ValidationErrorResponse GetExamples()
        {
            return new ValidationErrorResponse
            {
                Title = "Validation Error",
                Status = 422,
                Detail = "Validation errors occurred for type PaymentRequestCommand",
                Errors = new ErrorDetails
                {
                    ApiErrorDetailsList =
                    [
                        new ApiErrorDetail
                        {
                            MetaData = new MetaData
                            {
                                Property = "Cvv", ErrorMessage = "CVV must be 3 or 4 digits long"
                            }
                        },

                        new ApiErrorDetail
                        {
                            MetaData = new MetaData
                            {
                                Property = "Cvv",
                                ErrorMessage = "CVV must contain only numeric characters and be 3 or 4 digits"
                            }
                        }
                    ]
                }
            };
        }
    }
}
