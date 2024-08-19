using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Serilog;

public class EventHubSimulatorService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger _logger;

    public EventHubSimulatorService(
        QueueServiceClient queueServiceClient,
        IConfiguration configuration,
        ILogger logger)
    {
        var queueName = configuration["AzureStorage:EventQueueName"];
        _queueClient = queueServiceClient.GetQueueClient(queueName);
        _queueClient.CreateIfNotExists();
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, string identifier, CancellationToken cancellationToken)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        _logger.Information("Sending Payment To Events To Be Processed: {Identifier}", identifier);
        await _queueClient.SendMessageAsync(jsonMessage, cancellationToken: cancellationToken);
    }
}
