using MediatR;
using Microsoft.Extensions.Options;
using Sqs.Consumer.Client.Messages;

namespace Sqs.Consumer.Client.Handlers;

public class CustomerCreatedHandler: IRequestHandler<CustomerCreated>
{
    private readonly ILogger<CustomerCreatedHandler> _logger;
    private readonly IOptions<QueueOptions> _options;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger, IOptions<QueueOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public Task Handle(CustomerCreated request, CancellationToken cancellationToken)
    {
        if (_options.Value.AllowDeadLetterTest)
        {
            throw new Exception("Dead Letter Test");
        }

        _logger.LogInformation(request.FullName);
        return Unit.Task;
    }
}