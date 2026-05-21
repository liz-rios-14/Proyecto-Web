namespace SalesPoint.Application.DTOs.Invoices;

public sealed class CreateInvoiceRequest
{
    public int CustomerId { get; set; }
    public List<CreateInvoiceDetailRequest> Details { get; set; } = new();
}

public sealed class CreateInvoiceDetailRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}