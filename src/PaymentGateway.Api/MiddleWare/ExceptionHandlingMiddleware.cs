using System.Text.Json;

using FluentValidation;
using FluentValidation.Results;

using PaymentGateway.Api.Contracts;

namespace PaymentGateway.Api.MiddleWare
{
    internal sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var apiResponseError = new ApiError() { ApiErrorDetailsList = [] };
            var statusCode = GetStatusCode(exception);

            apiResponseError = exception switch
            {
                ValidationException validationException => new ApiError()
                {
                    ApiErrorDetailsList = validationException.Errors.Select(RetrieveErrorDetails).ToList()
                },
                _ => apiResponseError
            };

            var response = new
            {
                title = GetTitle(exception),
                status = statusCode,
                detail = exception.Message,
                errors = apiResponseError
            };

            httpContext.Response.ContentType = "application/json";

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                ValidationException => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            };

        private static string GetTitle(Exception exception) =>
            exception switch
            {
                ValidationException => "Validation Error",
                _ => "Server Error"
            };

        private static ApiErrorDetails RetrieveErrorDetails(ValidationFailure failure)
        {
            return new ApiErrorDetails
            {
                MetaData = new Dictionary<string, string>()
                {
                    { "Property", failure.PropertyName }, { "ErrorMessage", failure.ErrorMessage }
                }
            };
        }
    }
}
