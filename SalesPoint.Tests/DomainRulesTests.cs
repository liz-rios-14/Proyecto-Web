using SalesPoint.Application.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;
using Xunit;

namespace SalesPoint.Tests;

// Automated Tests
public sealed class DomainRulesTests
{
    [Fact]
    public void Invoice_DoesNotAllowDuplicateProduct()
    {
        var invoice = CreateInvoice();
        var product = new Product("CAFE", 2.50m, 10);
        SetEntityId(product, 1);

        invoice.AddDetail(product, 1);

        Assert.Throws<DuplicateProductException>(
            () => invoice.AddDetail(product, 1));
    }

    [Fact]
    public void Invoice_DoesNotAllowQuantityGreaterThanStock()
    {
        var invoice = CreateInvoice();
        var product = new Product("CAFE", 2.50m, 2);
        SetEntityId(product, 1);
        invoice.AddDetail(product, 3);

        Assert.Throws<InsufficientStockException>(() => invoice.Confirm());
    }

    [Fact]
    public void Invoice_CalculatesSubtotalTaxAndTotal()
    {
        var invoice = CreateInvoice();
        var product = new Product("CAFE", 10m, 5);
        SetEntityId(product, 1);
        invoice.AddDetail(product, 2);

        Assert.Equal(20m, invoice.Subtotal);
        Assert.Equal(2.40m, invoice.Tax);
        Assert.Equal(22.40m, invoice.Total);
    }

    [Fact]
    public void Invoice_DoesNotAllowInvalidCustomer()
    {
        Assert.Throws<DomainException>(() => new Invoice(0));
    }

    [Theory]
    [InlineData("corta1!", false)]
    [InlineData("SINMIN1!", false)]
    [InlineData("SinNumero!", false)]
    [InlineData("Segura1!", true)]
    public void PasswordPolicy_IsValidated(string password, bool valid)
    {
        Assert.Equal(
            valid,
            AuthService.GetPasswordValidationMessage(password) is null);
    }

    [Theory]
    [InlineData("correo-invalido")]
    [InlineData("@dominio.com")]
    public void Email_IsValidated(string email)
    {
        Assert.Throws<DomainException>(() => ApplicationValidator.Email(email));
    }

    [Fact]
    public void Customer_CanBeDeactivatedWithoutBeingDeleted()
    {
        var customer = new Customer(
            "JUAN",
            "PEREZ",
            "1710034065",
            "0999999999",
            "QUITO CENTRO",
            "juan@example.com");

        customer.Deactivate();

        Assert.False(customer.IsActive);
        Assert.False(customer.IsDeleted);
    }

    private static Invoice CreateInvoice()
    {
        var invoice = new Invoice(1);
        invoice.AssignInvoiceNumber(1);
        invoice.SetAuditSnapshot(
            "JUAN PEREZ",
            "1710034065",
            "juan@example.com",
            "0999999999",
            "QUITO CENTRO",
            1,
            "seller",
            "VENDEDOR",
            "SELLER");
        return invoice;
    }

    private static void SetEntityId(object entity, int id)
    {
        var property = entity.GetType().BaseType?.GetProperty("Id");
        property?.SetValue(entity, id);
    }
}
