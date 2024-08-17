using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public interface IHandlePaymentGateway
    {
        public Task HandlePyament(PaymentDto paymentDto, CancellationToken stoppingToken);
    }
}