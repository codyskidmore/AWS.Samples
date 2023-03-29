using Customers.Api.Contracts.Data;

namespace Customers.Api.Repositories;

public interface ICustomerRepository
{
    Task<bool> CreateAsync(CustomerDto customer, CancellationToken token);

    Task<CustomerDto?> GetAsync(Guid id, CancellationToken token);

    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken token);

    Task<bool> UpdateAsync(CustomerDto customer, CancellationToken token);

    Task<bool> DeleteAsync(Guid id, CancellationToken token);
}
