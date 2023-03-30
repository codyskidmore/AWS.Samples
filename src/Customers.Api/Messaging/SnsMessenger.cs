using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Customers.Api.Contracts.Messaging;
using Microsoft.Extensions.Options;

namespace Customers.Api.Messaging;

public class SnsMessenger : ISnsMessenger
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly IOptions<TopicOptions> _topicOptions;
    private string? _topicArn; 

    public SnsMessenger(IAmazonSimpleNotificationService sns, IOptions<TopicOptions> topicOptions)
    {
        _sns = sns;
        _topicOptions = topicOptions;
    }

    public async Task<PublishResponse> PublishMessageAsync<T>(T message, CancellationToken token)
    {
        var topicArn = await GetTopicArnAsync();
        var publishRequest = new PublishRequest
        {
            TopicArn = topicArn,
            Message = JsonSerializer.Serialize(message),
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

        return await _sns.PublishAsync(publishRequest, token);
    }

    private async ValueTask<string> GetTopicArnAsync()
    {
        if (_topicArn is not null)
        {
            return _topicArn;
        }
        var topicArnResponse = await _sns.FindTopicAsync(_topicOptions.Value.TopicName);
        _topicArn = topicArnResponse.TopicArn;
        return _topicArn;
    }
}