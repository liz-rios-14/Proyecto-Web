using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class AuditInvoiceHistory : BaseEntity
{
    private AuditInvoiceHistory()
    {
    }

    public AuditInvoiceHistory(
        string originalInvoiceNumber,
        int generatedInvoiceId,
        string generatedInvoiceNumber,
        int generatedByUserId,
        decimal total)
    {
        if (string.IsNullOrWhiteSpace(originalInvoiceNumber))
            throw new DomainException("La factura original de auditoria es obligatoria.");

        if (generatedInvoiceId <= 0)
            throw new DomainException("La factura generada de auditoria es obligatoria.");

        if (string.IsNullOrWhiteSpace(generatedInvoiceNumber))
            throw new DomainException("El numero de factura generada es obligatorio.");

        if (generatedByUserId <= 0)
            throw new DomainException("El usuario que genera auditoria es obligatorio.");

        OriginalInvoiceNumber = originalInvoiceNumber.Trim().ToUpperInvariant();
        GeneratedInvoiceId = generatedInvoiceId;
        GeneratedInvoiceNumber = generatedInvoiceNumber.Trim().ToUpperInvariant();
        GeneratedByUserId = generatedByUserId;
        Total = total;
        GeneratedAt = DateTime.UtcNow;
    }

    public string OriginalInvoiceNumber { get; private set; } = string.Empty;
    public int GeneratedInvoiceId { get; private set; }
    public string GeneratedInvoiceNumber { get; private set; } = string.Empty;
    public int GeneratedByUserId { get; private set; }
    public decimal Total { get; private set; }
    public DateTime GeneratedAt { get; private set; }
}
