using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Cedula { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private Customer()
    {
    }

    public Customer(
        string firstName,
        string lastName,
        string cedula,
        string phone,
        string address,
        string email)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetCedula(cedula);
        SetPhone(phone);
        SetAddress(address);
        SetEmail(email);
    }

    public void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre del cliente es obligatorio.");

        var cleanName = NormalizeSpaces(firstName).ToUpperInvariant();

        if (cleanName.Length < 2)
            throw new DomainException("El nombre debe tener al menos 2 caracteres.");

        if (cleanName.Length > 40)
            throw new DomainException("El nombre no puede superar los 40 caracteres.");

        if (!Regex.IsMatch(cleanName, @"^[A-ZÁÉÍÓÚÜÑ ]+$"))
            throw new DomainException("El nombre solo puede contener letras.");

        FirstName = cleanName;
    }

    public void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido del cliente es obligatorio.");

        var cleanLastName = NormalizeSpaces(lastName).ToUpperInvariant();

        if (cleanLastName.Length < 2)
            throw new DomainException("El apellido debe tener al menos 2 caracteres.");

        if (cleanLastName.Length > 40)
            throw new DomainException("El apellido no puede superar los 40 caracteres.");

        if (!Regex.IsMatch(cleanLastName, @"^[A-ZÁÉÍÓÚÜÑ ]+$"))
            throw new DomainException("El apellido solo puede contener letras.");

        LastName = cleanLastName;
    }

    public void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("El teléfono del cliente es obligatorio.");

        var cleanPhone = phone.Trim();

        if (!Regex.IsMatch(cleanPhone, @"^09\d{8}$"))
            throw new DomainException("Ingrese un número de teléfono válido.");

        Phone = cleanPhone;
    }

    public void SetCedula(string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            throw new DomainException("La cédula del cliente es obligatoria.");

        var cleanCedula = cedula.Trim();

        if (!IsValidEcuadorianCedula(cleanCedula))
            throw new DomainException("Ingrese una cédula ecuatoriana válida.");

        Cedula = cleanCedula;
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("La dirección del cliente es obligatoria.");

        var cleanAddress = NormalizeSpaces(address).ToUpperInvariant();

        if (cleanAddress.Length < 5)
            throw new DomainException("La dirección es demasiado corta.");

        if (cleanAddress.Length > 150)
            throw new DomainException("La dirección no puede superar los 150 caracteres.");

        if (!Regex.IsMatch(cleanAddress, @"^[A-ZÁÉÍÓÚÜÑ0-9 .,#\-]+$"))
            throw new DomainException("La dirección contiene caracteres no permitidos.");

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
        string cedula,
        string phone,
        string address,
        string email)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetCedula(cedula);
        SetPhone(phone);
        SetAddress(address);
        SetEmail(email);
    }

    public void SoftDelete()
    {
        IsActive = false;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeSpaces(string value)
    {
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private static bool IsValidEcuadorianCedula(string cedula)
    {
        if (!Regex.IsMatch(cedula, @"^\d{10}$"))
            return false;

        var province = int.Parse(cedula[..2]);
        var digits = cedula.Select(character => character - '0').ToArray();

        if (province is < 1 or > 24 || digits[2] >= 6)
            return false;

        var sum = 0;

        for (var index = 0; index < 9; index++)
        {
            var value = digits[index];

            if (index % 2 == 0)
            {
                value *= 2;

                if (value > 9)
                    value -= 9;
            }

            sum += value;
        }

        return (10 - sum % 10) % 10 == digits[9];
    }
}
