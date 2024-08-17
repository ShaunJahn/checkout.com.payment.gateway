using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Newtonsoft.Json;

public class EventHubListenerService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger _logger;

    public EventHubListenerService(QueueServiceClient queueServiceClient, IConfiguration configuration, ILogger logger)
    {
        var queueName = configuration["AzureStorage:QueueName"];
        _queueClient = queueServiceClient.GetQueueClient(queueName);
        _queueClient.CreateIfNotExists();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await _queueClient.ReceiveMessageAsync();

            if (message.Value != null)
            {
                var eventData = JsonConvert.DeserializeObject<EventData>(message.Value.MessageText);

                TriggerWebhook(eventData!);

                await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
            }
        }
    }

    private void TriggerWebhook(EventData eventData)
    {
        _logger.Information("Triggered webhook for event: {Event}", eventData.Id);
    }
}
