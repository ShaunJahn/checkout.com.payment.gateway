using System.Diagnostics;

using FluentValidation;

using MediatR;

using ValidationException = FluentValidation.ValidationException;

namespace PaymentGateway.Api.Application
{
    public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var type = request.GetType().Name;
            var stopwatch = Stopwatch.StartNew();
            if (!validators.Any())
            {
                return await next();
            }

            var errorList = validators
                .Select(x => x.Validate(request))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (errorList.Any())
            {
                throw new ValidationException($"Validation errors occurred for type {type}", errorList);
            }

            return await next();
        }
    }
}