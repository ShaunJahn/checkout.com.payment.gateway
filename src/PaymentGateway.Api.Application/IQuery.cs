using MediatR;

namespace PaymentGateway.Api.Application
{
    public interface IQuery<out TResponse> : IRequest<TResponse>;
}
