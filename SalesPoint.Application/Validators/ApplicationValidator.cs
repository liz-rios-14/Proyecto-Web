using System.Text.RegularExpressions;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Validators;

// ========================================
// NUEVO CAMBIO - APPLICATION LAYER
// Autor: Andrew
// Descripción: Validadores centralizados para que los Controllers no tengan lógica de negocio.
// ========================================
public static class ApplicationValidator
{
    public static void Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} es obligatorio.");
    }

    public static void Positive(decimal value, string fieldName)
    {
        if (value <= 0)
            throw new DomainException($"{fieldName} debe ser mayor a cero.");
    }

    public static void Positive(int value, string fieldName)
    {
        if (value <= 0)
            throw new DomainException($"{fieldName} debe ser mayor a cero.");
    }

    public static void NotNegative(int value, string fieldName)
    {
        if (value < 0)
            throw new DomainException($"{fieldName} no puede ser negativo.");
    }

    public static void Email(string? email)
    {
        Required(email, "El correo");
        if (!Regex.IsMatch(email!.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("El correo no tiene un formato válido.");
    }

    public static void EcuadorianCedula(string? cedula)
    {
        Required(cedula, "La cédula");
        var clean = cedula!.Trim();
        if (!Regex.IsMatch(clean, @"^\d{10}$"))
            throw new DomainException("La cédula debe tener exactamente 10 dígitos.");

        var province = int.Parse(clean[..2]);
        if (province < 1 || province > 24)
            throw new DomainException("La cédula no tiene una provincia válida.");

        var digits = clean.Select(c => c - '0').ToArray();

        if (digits[2] >= 6)
            throw new DomainException("Ingrese una cédula ecuatoriana válida.");

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            var value = digits[i];
            if (i % 2 == 0)
            {
                value *= 2;
                if (value > 9) value -= 9;
            }
            sum += value;
        }
        var verifier = (10 - (sum % 10)) % 10;
        if (verifier != digits[9])
            throw new DomainException("La cédula no es válida.");
    }
}
