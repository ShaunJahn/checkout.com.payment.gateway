using System.Threading;

using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.Infrastructure
{
    public interface IPaymentRepository
    {
        Task<PaymentDto> UpsertPaymentAsync(PaymentDto payment, CancellationToken cancellationToken);
        Task UpdatePaymentStatusAsync(PaymentDto paymentUpdate, CancellationToken cancellationToken);
        Task<PaymentDto> GetPaymentByIdAsync(string id, CancellationToken cancellationToken);
    }
}
