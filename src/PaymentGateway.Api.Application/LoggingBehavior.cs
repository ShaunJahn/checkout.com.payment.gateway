using MediatR;

using Serilog;

namespace PaymentGateway.Api.Application
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly ILogger _logger;

        public LoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.Information("Starting request: {RequestName} at {DateTime}", requestName, DateTime.UtcNow);

            try
            {
                var response = await next();
                _logger.Information("Completed request: {RequestName} at {DateTime}", requestName, DateTime.UtcNow);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Request {RequestName} failed at {DateTime}", requestName, DateTime.UtcNow);
                throw;
            }
        }
    }
}
