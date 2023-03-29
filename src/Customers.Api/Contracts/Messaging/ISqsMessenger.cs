using Amazon.SQS.Model;

namespace Customers.Api.Contracts.Messaging;

public interface ISqsMessenger
{
    Task<SendMessageResponse> SendMessageAsync<T>(T message, CancellationToken token); // where T : IMessage;
}