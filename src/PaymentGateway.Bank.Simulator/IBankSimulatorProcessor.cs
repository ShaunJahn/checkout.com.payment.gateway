using PaymentGateway.Contracts.V1.Dtos;

namespace PaymentGateway.Bank.Simulator
{
    public interface IBankSimulatorProcessor
    {
        Task<AuthorizePaymentResponse> AuthorizePaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken);
    }
}
