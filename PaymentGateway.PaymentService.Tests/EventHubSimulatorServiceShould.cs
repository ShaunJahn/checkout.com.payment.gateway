using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

using Moq;

using PaymentGateway.Contracts.V1.Dtos;

using Serilog;

using Xunit;

namespace PaymentGateway.PaymentService.Tests
{
    public class EventHubSimulatorServiceShould
    {
        private readonly Mock<QueueServiceClient> _mockQueueServiceClient;
        private readonly Mock<QueueClient> _mockQueueClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EventHubSimulatorService _service;

        public EventHubSimulatorServiceShould()
        {
            _mockQueueServiceClient = new Mock<QueueServiceClient>();
            _mockQueueClient = new Mock<QueueClient>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(config => config["AzureStorage:QueueName"])
                              .Returns("payment-queue");

            _mockQueueServiceClient.Setup(client => client.GetQueueClient(It.IsAny<string>()))
                                   .Returns(_mockQueueClient.Object);

            _service = new EventHubSimulatorService(_mockQueueServiceClient.Object, _mockConfiguration.Object, _mockLogger.Object);
        }
        [Fact]
        public async Task SendPaymentAsync_ShouldSendPaymentToQueue_WhenCalled()
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

            // Act
            await _service.SendMessageAsync(paymentDto, paymentDto.id, CancellationToken.None);

            // Assert
            _mockQueueClient.Verify(client => client.SendMessageAsync(It.Is<string>(message =>
                message.Contains(paymentDto.id) && message.Contains("100")),
                It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}