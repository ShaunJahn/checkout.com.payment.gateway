using System.Net.Http;
using System.Text;

using Azure.Core.Pipeline;
using Azure.Storage.Queues;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

using Newtonsoft.Json;

using PaymentGateway.Contracts.Exceptions;

using Polly;
using Polly.Extensions.Http;

using Serilog;

public class EventHubListenerService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public EventHubListenerService(
        QueueServiceClient queueServiceClient,
        IConfiguration configuration,
        ILogger logger)
    {
        var queueName = configuration["AzureStorage:QueueName"];
        _queueClient = queueServiceClient.GetQueueClient(queueName);
        _queueClient.CreateIfNotExists();
        _logger = logger;

        var retryPolicy = GetRetryPolicy(_logger);
        _httpClient = new HttpClient(new PolicyHttpMessageHandler(retryPolicy) { InnerHandler = new HttpClientHandler() });
    }

    private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                2,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.Warning($"Retry {retryAttempt} for {context.PolicyKey} due to {outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase}");
                });
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(2000);
            var message = await _queueClient.ReceiveMessageAsync();

            if (message.Value != null)
            {
                var eventData = JsonConvert.DeserializeObject<EventData>(message.Value.MessageText);

                if (eventData == null)
                {
                    await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, cancellationToken);
                    continue;
                }
                try
                {
                    await TriggerWebhook(eventData, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Webhook sent to dead letter queue.");
                }
                finally
                {
                    await _queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, cancellationToken);
                    _logger.Information("Webhook Messaged Completed");
                }
            }
        }
    }

    private async Task TriggerWebhook(EventData eventData, CancellationToken cancellationToken)
    {
        _logger.Information("Triggered webhook for event: {Event} with status {Status}", eventData.Id, eventData.Status);

        var webhookUrl = "https://webhook-test.com/b761223986dfbcc568901a8791442a50";
        var jsonRequest = JsonConvert.SerializeObject(eventData);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        _logger.Information("Webhook sent for event: {Event}", eventData.Id);

        await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
    }
}
