using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using MediatR;
using Microsoft.Extensions.Options;
using Sqs.Consumer.Client.Messages;

namespace Sqs.Consumer.Client;

public class SqsConsumerService : BackgroundService
{
    private readonly IAmazonSQS _amazonSqs;
    private readonly IOptions<QueueOptions> _options;
    private readonly IMediator _mediator;
    private readonly ILogger<SqsConsumerService> _logger;

    public SqsConsumerService(IOptions<QueueOptions> options, IAmazonSQS amazonSqs, 
        IMediator mediator, ILogger<SqsConsumerService> logger)
    {
        _options = options;
        _amazonSqs = amazonSqs;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrlResponse = await _amazonSqs.GetQueueUrlAsync(_options.Value.QueueName, stoppingToken);

        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            AttributeNames = new List<string>{ "All" }, // or specific a list of specific ones
            MessageAttributeNames = new List<string>{ "All" },
            MaxNumberOfMessages = 1  // Is actually the default. Just here to point out feature.
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _amazonSqs.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var m in response.Messages)
            {
                var messageType = m.MessageAttributes["MessageType"].StringValue;
                
                // This is flawed. If the namespace changes, the code will break. 
                var type = Type.GetType($"{_options.Value.MessageTypeNamespace}.{messageType}");
                
                if (type is null)
                {
                    _logger.LogWarning($"Unknown message type: {messageType}. You man need to check the message type namespace setting");
                    continue;
                }

                var message = (ISqsMessage)JsonSerializer.Deserialize(m.Body, type)!;

                try
                {
                    await _mediator.Send(message, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to process message type: {messageType}");
                    continue;
                }
                
                await _amazonSqs.DeleteMessageAsync(queueUrlResponse.QueueUrl, m.ReceiptHandle, stoppingToken);
            }

            await Task.Delay(_options.Value.WaitPeriod, stoppingToken);
        }
    }
}