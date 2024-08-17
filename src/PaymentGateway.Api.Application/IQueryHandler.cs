using MediatR;

namespace PaymentGateway.Api.Application
{
    internal interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>;
}
