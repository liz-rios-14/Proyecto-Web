using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private Customer()
    {
    }

    public Customer(
        string firstName,
        string lastName,
        string phone,
        string address,
        string email)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetPhone(phone);
        SetAddress(address);
        SetEmail(email);
    }

    public void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre del cliente es obligatorio.");

        var cleanName = firstName.Trim().ToUpperInvariant();

        if (cleanName.Length < 2)
            throw new DomainException("El nombre debe tener al menos 2 caracteres.");

        if (cleanName.Length > 40)
            throw new DomainException("El nombre no puede superar los 40 caracteres.");

        FirstName = cleanName;
    }

    public void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido del cliente es obligatorio.");

        var cleanLastName = lastName.Trim().ToUpperInvariant();

        if (cleanLastName.Length < 2)
            throw new DomainException("El apellido debe tener al menos 2 caracteres.");

        if (cleanLastName.Length > 40)
            throw new DomainException("El apellido no puede superar los 40 caracteres.");

        LastName = cleanLastName;
    }

    public void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("El teléfono del cliente es obligatorio.");

        var cleanPhone = phone.Trim();

        if (!Regex.IsMatch(cleanPhone, @"^\d{10}$"))
            throw new DomainException("El teléfono debe contener exactamente 10 dígitos.");

        Phone = cleanPhone;
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("La dirección del cliente es obligatoria.");

        var cleanAddress = address.Trim().ToUpperInvariant();

        if (cleanAddress.Length < 5)
            throw new DomainException("La dirección es demasiado corta.");

        if (cleanAddress.Length > 150)
            throw new DomainException("La dirección no puede superar los 150 caracteres.");

        Address = cleanAddress;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo del cliente es obligatorio.");

        var cleanEmail = email.Trim().ToLowerInvariant();

        if (cleanEmail.Length > 120)
            throw new DomainException("El correo es demasiado largo.");

        if (!Regex.IsMatch(
                cleanEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new DomainException("El correo del cliente no es válido.");
        }

        Email = cleanEmail;
    }

    public void Update(
        string firstName,
        string lastName,
        string phone,
        string address,
        string email)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetPhone(phone);
        SetAddress(address);
        SetEmail(email);
    }
}