using Customers.Api.Domain;
using Customers.Api.Mapping;
using Customers.Api.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Customers.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task<bool> CreateAsync(Customer customer)
    {
        var existingUser = await _customerRepository.GetAsync(customer.Id.Value);
        if (existingUser is not null)
        {
            var message = "$a user with id {customer.Id} already exists";
            throw new ValidationException(message, new []
            {
                new ValidationFailure(nameof(Customer), message)
            });
        }
        var customerDto = customer.ToCustomerDto();
        return await _customerRepository.CreateAsync(customerDto);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Customer>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Customer?> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Customer customer)
    {
        throw new NotImplementedException();
    }
}