using MediatR;

namespace PaymentGateway.Api.Application
{
    public interface ICommand<out TResponse> : IRequest<TResponse>;
}
