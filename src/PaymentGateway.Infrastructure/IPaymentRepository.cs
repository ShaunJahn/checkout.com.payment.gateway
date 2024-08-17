using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.Infrastructure
{
    public interface IPaymentRepository
    {
        Task<PaymentDto> UpsertPaymentAsync(PaymentDto payment);
        Task UpdatePaymentStatusAsync(string id, string status);
        Task<PaymentDto> GetPaymentByIdAsync(string id);
    }
}
