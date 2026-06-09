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
        int pageSize,
        bool onlyActive)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var data = await _repository.SearchAsync(field, value, pageNumber, pageSize, onlyActive);
        var totalItems = await _repository.CountAsync(field, value, onlyActive);

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
        if (request is null)
            throw new DomainException("Los datos del cliente son obligatorios.");

        ApplicationValidator.Required(request.FirstName, "El nombre del cliente");
        ApplicationValidator.Required(request.LastName, "El apellido del cliente");
        ApplicationValidator.EcuadorianCedula(request.Cedula);
        ApplicationValidator.Email(request.Email);

        if (await _repository.ExistsByEmailAsync(request.Email))
            throw new DomainException("Ya existe un cliente con el mismo correo.");

        if (await _repository.ExistsByCedulaAsync(request.Cedula))
            throw new DomainException("Ya existe un cliente registrado con esta cedula.");

        var customer = new Customer(
            request.FirstName,
            request.LastName,
            request.Cedula,
            request.Phone,
            request.Address,
            request.Email
        );

        await _repository.CreateAsync(customer);

        return Map(customer);
    }

    public async Task UpdateAsync(int id, UpdateCustomerRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del cliente son obligatorios.");

        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        ApplicationValidator.Required(request.FirstName, "El nombre del cliente");
        ApplicationValidator.Required(request.LastName, "El apellido del cliente");
        ApplicationValidator.EcuadorianCedula(request.Cedula);
        ApplicationValidator.Email(request.Email);

        if (await _repository.ExistsByEmailAsync(request.Email, id))
            throw new DomainException("Ya existe otro cliente con el mismo correo.");

        if (await _repository.ExistsByCedulaAsync(request.Cedula, id))
            throw new DomainException("Ya existe otro cliente registrado con esta cedula.");

        customer.Update(
            request.FirstName,
            request.LastName,
            request.Cedula,
            request.Phone,
            request.Address,
            request.Email
        );

        await _repository.UpdateAsync(customer);
    }

    public async Task<DeleteResultDto> DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        if (await _repository.HasHistoryAsync(id))
            throw new DomainException("No se puede eliminar fisicamente el cliente porque tiene historial. Use Desactivar para conservar la auditoria.");

        await _repository.DeleteAsync(customer);

        return new DeleteResultDto
        {
            WasPhysical = true,
            Message = "Cliente eliminado fisicamente porque no tenia historial asociado."
        };
    }

    public async Task<DeleteResultDto> DeactivateAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        customer.Deactivate();
        await _repository.UpdateAsync(customer);

        return new DeleteResultDto
        {
            WasPhysical = false,
            Message = "Cliente desactivado correctamente. Permanecera visible para auditoria."
        };
    }

    public async Task<DeleteResultDto> ActivateAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Cliente no encontrado.");

        customer.Activate();
        await _repository.UpdateAsync(customer);

        return new DeleteResultDto
        {
            WasPhysical = false,
            Message = "Cliente activado correctamente."
        };
    }
    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Cedula = customer.Cedula ?? string.Empty,
            Phone = customer.Phone,
            Address = customer.Address,
            Email = customer.Email,
            IsActive = customer.IsActive
        };
    }
}
