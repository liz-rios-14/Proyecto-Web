namespace SalesPoint.Application.DTOs.Invoices;

public sealed class CreateInvoiceRequest
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCedula { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string AuditSourceInvoiceNumber { get; set; } = string.Empty;

    public List<CreateInvoiceDetailRequest> Details { get; set; } = new();
}

public sealed class CreateInvoiceDetailRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
