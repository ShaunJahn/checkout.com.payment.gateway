using Azure.Storage.Queues;
using Newtonsoft.Json;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Contracts.V1;
using System.Diagnostics;

namespace PaymentGateway.PaymentService.PaymentProcessor
{
    public class ProcessPaymentsHandler : IHandlePaymentGateway
    {
        public ProcessPaymentsHandler()
        {

        }
        public Task HandlePyament(PaymentDto payment, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
