using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.Infrastructure
{
    public interface IPaymentRepository
    {
        Task<PaymentDto> UpsertPaymentAsync(PaymentDto payment);
        Task UpdatePaymentStatusAsync(PaymentDto paymentUpdate);
        Task<PaymentDto> GetPaymentByIdAsync(string id);
    }
}
