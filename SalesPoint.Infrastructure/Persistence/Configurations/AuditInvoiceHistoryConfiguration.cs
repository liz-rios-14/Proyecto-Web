using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class AuditInvoiceHistoryConfiguration : IEntityTypeConfiguration<AuditInvoiceHistory>
{
    public void Configure(EntityTypeBuilder<AuditInvoiceHistory> builder)
    {
        builder.ToTable("audit_invoice_histories");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id).HasColumnName("id");
        builder.Property(history => history.OriginalInvoiceNumber)
            .HasColumnName("original_invoice_number")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(history => history.GeneratedInvoiceId)
            .HasColumnName("generated_invoice_id")
            .IsRequired();
        builder.Property(history => history.GeneratedInvoiceNumber)
            .HasColumnName("generated_invoice_number")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(history => history.GeneratedByUserId)
            .HasColumnName("generated_by_user_id")
            .IsRequired();
        builder.Property(history => history.Total)
            .HasColumnName("total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(history => history.GeneratedAt)
            .HasColumnName("generated_at")
            .IsRequired();
        builder.Property(history => history.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(history => history.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(history => history.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(history => history.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(history => history.OriginalInvoiceNumber);
        builder.HasIndex(history => history.GeneratedInvoiceNumber).IsUnique();
    }
}
