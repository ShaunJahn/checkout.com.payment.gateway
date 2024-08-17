using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class EventHubSimulatorService
{
    private readonly QueueClient _queueClient;

    public EventHubSimulatorService(QueueServiceClient queueServiceClient, IConfiguration configuration)
    {
        var queueName = configuration["AzureStorage:QueueName"];
        _queueClient = queueServiceClient.GetQueueClient(queueName);
        _queueClient.CreateIfNotExists();
    }

    public async Task SendMessageAsync<T>(T message)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        await _queueClient.SendMessageAsync(jsonMessage);
    }
}
