using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Customers;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<CustomerDto>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var data = await _repository.SearchAsync(field, value, pageNumber, pageSize);
        var totalItems = await _repository.CountAsync(field, value);

        var items = data.Select(Map).ToList();

        return new PagedResponse<CustomerDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
    {
        ApplicationValidator.Required(request.FirstName, "El nombre del cliente");
        ApplicationValidator.Required(request.LastName, "El apellido del cliente");
        ApplicationValidator.Email(request.Email);

        if (await _repository.ExistsByEmailAsync(request.Email))
            throw new DomainException("Ya existe un cliente con el mismo correo.");

        var customer = new Customer(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            request.Email
        );

        await _repository.CreateAsync(customer);

        return Map(customer);
    }

    public async Task UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        ApplicationValidator.Required(request.FirstName, "El nombre del cliente");
        ApplicationValidator.Required(request.LastName, "El apellido del cliente");
        ApplicationValidator.Email(request.Email);

        if (await _repository.ExistsByEmailAsync(request.Email, id))
            throw new DomainException("Ya existe otro cliente con el mismo correo.");

        customer.Update(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            request.Email
        );

        await _repository.UpdateAsync(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        await _repository.DeleteAsync(customer);
    }

    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Address = customer.Address,
            Email = customer.Email
        };
    }
}