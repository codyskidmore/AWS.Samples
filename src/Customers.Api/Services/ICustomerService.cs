using Customers.Api.Domain;

namespace Customers.Api.Services;

public interface ICustomerService
{
    Task<bool> CreateAsync(Customer customer, CancellationToken token);

    Task<Customer?> GetAsync(Guid id, CancellationToken token);

    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken token);

    Task<bool> UpdateAsync(Customer customer, CancellationToken token);

    Task<bool> DeleteAsync(Guid id, CancellationToken token);
}
