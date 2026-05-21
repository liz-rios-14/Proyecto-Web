using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Customers;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
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
        var data = await _repository.SearchAsync(field, value);

        var totalItems = data.Count;

        var items = data
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Address = customer.Address,
                Email = customer.Email
            })
            .ToList();

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
        var entity = await _repository.GetByIdAsync(id);

        if (entity == null) return null;

        return new CustomerDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            Address = entity.Address,
            Email = entity.Email
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
    {
        var entity = new Customer(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            request.Email
        );

        var created = await _repository.CreateAsync(entity);

        return new CustomerDto
        {
            Id = created.Id,
            FirstName = created.FirstName,
            LastName = created.LastName,
            Phone = created.Phone,
            Address = created.Address,
            Email = created.Email
        };
    }

    public async Task UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity == null)
            throw new DomainException("Cliente no encontrado.");

        entity.Update(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            request.Email
        );

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity == null)
            throw new DomainException("Cliente no encontrado.");

        await _repository.DeleteAsync(entity);
    }
}