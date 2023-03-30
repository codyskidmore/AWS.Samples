using Customers.Api.Contracts.Messaging;
using Customers.Api.Domain;
using Customers.Api.Mapping;
using Customers.Api.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Customers.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGitHubService _gitHubService;
    private readonly ISnsMessenger _snsMessenger;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository customerRepository, 
        IGitHubService gitHubService, ISnsMessenger snsMessenger, ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _gitHubService = gitHubService;
        _snsMessenger = snsMessenger;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(Customer customer, CancellationToken token)
    {
        var existingUser = await _customerRepository.GetAsync(customer.Id, token);
        if (existingUser is not null)
        {
            var message = $"A user with id {customer.Id} already exists";
            throw new ValidationException(message, GenerateValidationError(nameof(Customer), message));
        }

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }
        
        var customerDto = customer.ToCustomerDto();
        
        // Add Cancellation token here also.
        var response = await _customerRepository.CreateAsync(customerDto, token);

        if (response)
        {
            await _snsMessenger.PublishMessageAsync(customer.ToCustomerCreatedMessage(), token);
            _logger.LogInformation($"Published Create message: {customerDto.FullName}");
        }

        return response;
    }

    public async Task<Customer?> GetAsync(Guid id, CancellationToken token)
    {
        var customerDto = await _customerRepository.GetAsync(id, token);
        return customerDto?.ToCustomer();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken token)
    {
        var customerDtos = await _customerRepository.GetAllAsync(token);
        return customerDtos.Select(x => x.ToCustomer());
    }

    public async Task<bool> UpdateAsync(Customer customer, CancellationToken token)
    {
        var customerDto = customer.ToCustomerDto();
        
        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }
        
        // Add Cancellation token here also.
        var response = await _customerRepository.UpdateAsync(customerDto, token);

        if (response)
        {
            await _snsMessenger.PublishMessageAsync(customer.ToCustomerUpdatedMessage(), token);
            _logger.LogInformation($"Published Update message: {customerDto.FullName}");
        }

        return response;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token)
    {
        var response = await _customerRepository.DeleteAsync(id, token);

        if (response)
        {
            await _snsMessenger.PublishMessageAsync(new CustomerDeleted
            {
                Id = id
            }, token);
        }

        return response;
    }

    private static ValidationFailure[] GenerateValidationError(string paramName, string message)
    {
        return new []
        {
            new ValidationFailure(paramName, message)
        };
    }
}
