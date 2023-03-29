namespace Sqs.Consumer.Client;

public class QueueOptions
{
    public required string QueueName { get; init; }
    public required int WaitPeriod { get; set; }
    public required string MessageTypeNamespace { get; set; }
}