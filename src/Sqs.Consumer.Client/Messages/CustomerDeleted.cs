namespace Sqs.Consumer.Client.Messages;

// Usually shared in a Nuget package.

public class CustomerDeleted : ISqsMessage
{
    public required Guid Id { get; init; }
}
