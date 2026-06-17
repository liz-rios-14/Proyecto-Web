namespace SalesPoint.Application.DTOs.Invoices;

public sealed class ReconstructedInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public ReconstructedCustomerDto Customer { get; set; } = new();
    public ReconstructedSellerDto Seller { get; set; } = new();

    public List<ReconstructedInvoiceDetailDto> Details { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public sealed class ReconstructedCustomerDto
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Cedula { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}

public sealed class ReconstructedSellerDto
{
    public int SellerId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class ReconstructedInvoiceDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
