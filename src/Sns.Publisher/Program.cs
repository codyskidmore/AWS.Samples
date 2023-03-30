using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Sns.Publisher;

var customer = new CustomerCreated
{
    Id = Guid.NewGuid(),
    GitHubUsername = "cody-at-github",
    FullName = "Cody Skidmore",
    Email = "cody@codewonk.com",
    DateOfBirth = new DateTime(1990, 05, 15)
};

var snsClient = new AmazonSimpleNotificationServiceClient();
// Amazon Resource Name (ARN), misspelled the name when creating SNS config. Should be "customers-sns"
var topicArnResponse = await snsClient.FindTopicAsync("customer-sns");

var publishRequest = new PublishRequest
{   
    TopicArn = topicArnResponse.TopicArn,
    Message = JsonSerializer.Serialize(customer),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType", 
            new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        }
    }
};

var response = await snsClient.PublishAsync(publishRequest);
