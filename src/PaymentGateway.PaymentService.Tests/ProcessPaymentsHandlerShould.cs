using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

using Moq;

using PaymentGateway.Bank.Simulator;
using PaymentGateway.Contracts.V1;
using PaymentGateway.Contracts.V1.Dtos;
using PaymentGateway.Infrastructure;
using PaymentGateway.PaymentService.PaymentProcessor;

using Serilog;

using Xunit;

namespace PaymentGateway.PaymentService.Tests
{
    public class ProcessPaymentsHandlerShould
    {
        private readonly Mock<QueueServiceClient> _mockQueueServiceClient;
        private readonly Mock<QueueClient> _mockQueueClient;
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly EventHubSimulatorService _mockEventHubSimulatorService;
        private readonly PaymentQueueService _mockPaymentQueueService;
        private readonly Mock<IBankSimulatorProcessor> _mockBankSimulatorProcessor;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ProcessPaymentsHandler _handler;

        public ProcessPaymentsHandlerShould()
        {
            _mockQueueServiceClient = new Mock<QueueServiceClient>();
            _mockQueueClient = new Mock<QueueClient>();
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockLogger = new Mock<ILogger>();

            _mockBankSimulatorProcessor = new Mock<IBankSimulatorProcessor>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(config => config["AzureStorage:QueueName"])
                              .Returns("payment-queue");

            _mockQueueServiceClient.Setup(client => client.GetQueueClient(It.IsAny<string>()))
                                   .Returns(_mockQueueClient.Object);

            _mockEventHubSimulatorService = new EventHubSimulatorService(
                 _mockQueueServiceClient.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);

            _mockPaymentQueueService = new PaymentQueueService(
                _mockQueueServiceClient.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );

            _handler = new ProcessPaymentsHandler(
                _mockQueueServiceClient.Object,
                _mockConfiguration.Object,
                _mockPaymentRepository.Object,
                _mockLogger.Object,
                _mockEventHubSimulatorService,
                _mockPaymentQueueService,
                _mockBankSimulatorProcessor.Object);
        }

        [Fact]
        public async Task HandlePayment_AuthorizePayment_AndSendToQueue_WhenPaymentIsAuthorized()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                id = "12345",
                Status = "Processing",
                AuthorizationCode = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Amount = 100,
                Currency = "USD",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            };

            var response = new AuthorizePaymentResponse
            {
                IsAuthorized = true,
                AuthorizationCode = Guid.NewGuid()
            };

            _mockBankSimulatorProcessor.Setup(x => x.AuthorizePaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(response);

            _mockPaymentRepository.Setup(x => x.UpdatePaymentStatusAsync(It.IsAny<PaymentDto>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.CompletedTask);

            // Act
            await _handler.HandlePayment(paymentDto, CancellationToken.None);

            // Assert
            _mockBankSimulatorProcessor.Verify(x => x.AuthorizePaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockPaymentRepository.Verify(x => x.UpdatePaymentStatusAsync(It.Is<PaymentDto>(p => p.Status == PaymentStatus.Authorized.ToString()), It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.Verify(x => x.Information("Payment created with PaymentId: {PaymentId} and Status: {Status}", paymentDto.id, paymentDto.Status), Times.Once);
        }

        [Fact]
        public async Task HandlePayment_DeclinePayment_WhenPaymentIsNotAuthorized()
        {
            // Arrange
            var paymentDto = new PaymentDto
            {
                id = "12345",
                Status = "Processing",
                AuthorizationCode = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Amount = 100,
                Currency = "USD",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            };
            var response = new AuthorizePaymentResponse
            {
                IsAuthorized = false
            };

            _mockBankSimulatorProcessor.Setup(x => x.AuthorizePaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(response);

            _mockPaymentRepository.Setup(x => x.UpdatePaymentStatusAsync(It.IsAny<PaymentDto>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.CompletedTask);

            // Act
            await _handler.HandlePayment(paymentDto, CancellationToken.None);

            // Assert
            _mockBankSimulatorProcessor.Verify(x => x.AuthorizePaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockPaymentRepository.Verify(x => x.UpdatePaymentStatusAsync(It.Is<PaymentDto>(p => p.Status == PaymentStatus.Declined.ToString()), It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.Verify(x => x.Information("Payment created with PaymentId: {PaymentId} and Status: {Status}", paymentDto.id, paymentDto.Status), Times.Once);
        }
    }
}