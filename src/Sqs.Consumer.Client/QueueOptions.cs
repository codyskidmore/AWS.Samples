namespace Sqs.Consumer.Client;

public class QueueOptions
{
    public required string QueueName { get; init; }
    public required int WaitPeriod { get; init; }
    public required string MessageTypeNamespace { get; init; }
    public required bool AllowDeadLetterTest { get; init; }
}