using Amazon.SimpleNotificationService.Model;

namespace Customers.Api.Contracts.Messaging;

public interface ISnsMessenger
{
    Task<PublishResponse> PublishMessageAsync<T>(T message, CancellationToken token); // where T : IMessage;
}