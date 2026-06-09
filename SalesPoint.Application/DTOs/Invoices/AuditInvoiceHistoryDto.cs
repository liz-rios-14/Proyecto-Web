namespace SalesPoint.Application.DTOs.Invoices;

public sealed class AuditInvoiceHistoryDto
{
    public int Id { get; set; }
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public int GeneratedInvoiceId { get; set; }
    public string GeneratedInvoiceNumber { get; set; } = string.Empty;
    public int GeneratedByUserId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public decimal Total { get; set; }
}
