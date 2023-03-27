// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sqs.Sample;

// var sqsClient1 = new AmazonSQSClient(new AmazonSQSConfig(
// {
//     ...
// }));

//var sqsClient2 = new AmazonSQSClient(RegionEndpoint.USEast2);

// Many other options/features available...

var customer = new CustomerCreated
{
    DateOfBirth = new DateTime(1990,05,15),
    Email = "cody@codewonk.com",
    FullName = "Cody Skidmore",
    GitHubUsername = "cody-skidmore-githubhuser",
    Id = Guid.NewGuid()
};

var sqsClient = new AmazonSQSClient();

var queueUrlResponse = await  sqsClient.GetQueueUrlAsync("customers-sqs");
// => queueUrlResponse.QueueUrl

var sendMesssageRequest = new SendMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(customer),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        }
    }
    //, DelaySeconds = ..
};

var response = await sqsClient.SendMessageAsync(sendMesssageRequest);

Console.WriteLine(response);

return; // allows us to pause and check results before exiting. 