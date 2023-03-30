using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Api.Contracts.Messaging;
using Microsoft.Extensions.Options;

namespace Customers.Api.Messaging;

public class SqsMessenger : ISqsMessenger
{
    private readonly IAmazonSQS _sqs;
    private readonly IOptions<QueueOptions> _queueSettings;
    private string? _queueUrl; 

    public SqsMessenger(IAmazonSQS sqs, IOptions<QueueOptions> queueSettings)
    {
        _sqs = sqs;
        _queueSettings = queueSettings;
    }

    public async Task<SendMessageResponse> SendMessageAsync<T>(T message, CancellationToken token)
    {
        var queueUrl = await GetQueueUrlAsync(token);
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "MessageType", new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = typeof(T).Name
                    }

                }
            }
        };

        return await _sqs.SendMessageAsync(sendMessageRequest, token);
    }

    private async Task<string> GetQueueUrlAsync(CancellationToken token)
    {
        if (_queueUrl is not null)
        {
            return _queueUrl;
        }
        var queueUrlResponse = await _sqs.GetQueueUrlAsync(_queueSettings.Value.QueueName, token);
        _queueUrl = queueUrlResponse.QueueUrl;
        return _queueUrl;
    }
}