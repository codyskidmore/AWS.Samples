// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sqs.Sample;

await RunConsumer();

return; // allows us to pause and check results before exiting. 

static async Task RunPublisher()
{
    // More options..
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
}

static async Task RunConsumer()
{
    var cts = new CancellationTokenSource();
    var sqsClient = new AmazonSQSClient();

    var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers-sqs");

    var receiveMessageRequest = new ReceiveMessageRequest
    {
        QueueUrl = queueUrlResponse.QueueUrl,
        AttributeNames = new List<string>{ "All" }, // or specific a list of specific ones
        MessageAttributeNames = new List<string>{ "All" }
    };

    while (!cts.IsCancellationRequested)
    {
        var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts.Token);
    
        foreach (var message in response.Messages)
        {
            Console.WriteLine($"Message Id: {message.MessageId}");
            Console.WriteLine($"Message Body: {message.Body}");

            await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle);
        }
        await Task.Delay(3000);
    }
}