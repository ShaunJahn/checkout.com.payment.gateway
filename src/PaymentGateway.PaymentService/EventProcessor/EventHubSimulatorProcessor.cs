using System.Text;

using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using Serilog;

public class EventHubListenerService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public EventHubListenerService(
        QueueServiceClient queueServiceClient,
        IConfiguration configuration,
        ILogger logger,
        HttpClient httpClient)
    {
        var queueName = configuration["AzureStorage:QueueName"];
        _queueClient = queueServiceClient.GetQueueClient(queueName);
        _queueClient.CreateIfNotExists();
        _logger = logger;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await _queueClient.ReceiveMessageAsync();

            if (message.Value != null)
            {
                var eventData = JsonConvert.DeserializeObject<EventData>(message.Value.MessageText);

                await TriggerWebhook(eventData, cancellationToken);

                await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
            }
        }
    }

    private async Task TriggerWebhook(EventData eventData, CancellationToken cancellationToken)
    {
        _logger.Information("Triggered webhook for event: {Event}", eventData.Id);

        var webhookUrl = "https://webhook.site/f13ca7c4-76f4-4c70-a884-8352e0206b7b";
        var jsonRequest = JsonConvert.SerializeObject(eventData);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
    }
}
