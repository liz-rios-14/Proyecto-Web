using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id");

        builder.Property(invoice => invoice.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(invoice => invoice.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(invoice => invoice.InvoiceNumber)
            .HasColumnName("invoice_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(invoice => invoice.InvoiceNumber)
            .IsUnique();

        builder.Ignore(invoice => invoice.Subtotal);

        builder.Ignore(invoice => invoice.Tax);

        builder.Ignore(invoice => invoice.Total);

        builder.Metadata
            .FindNavigation(nameof(Invoice.Details))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}