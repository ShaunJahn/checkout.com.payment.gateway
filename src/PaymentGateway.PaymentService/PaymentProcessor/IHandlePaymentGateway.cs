using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public interface IHandlePaymentGateway
    {
        public Task HandlePayment(PaymentDto paymentDto, CancellationToken stoppingToken);
    }
}