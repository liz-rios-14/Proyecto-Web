using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Enums;

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

        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<SaleStatus>(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerNameSnapshot)
            .HasColumnName("customer_name_snapshot")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerEmailSnapshot)
            .HasColumnName("customer_email_snapshot")
            .HasMaxLength(150);

        builder.Property(invoice => invoice.CustomerPhoneSnapshot)
            .HasColumnName("customer_phone_snapshot")
            .HasMaxLength(30);

        builder.Property(invoice => invoice.CustomerAddressSnapshot)
            .HasColumnName("customer_address_snapshot")
            .HasMaxLength(200);

        builder.Property(invoice => invoice.SellerId)
            .HasColumnName("seller_id")
            .IsRequired();

        builder.Property(invoice => invoice.SellerUserNameSnapshot)
            .HasColumnName("seller_username_snapshot")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(invoice => invoice.SellerFullNameSnapshot)
            .HasColumnName("seller_full_name_snapshot")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(invoice => invoice.SellerRoleSnapshot)
            .HasColumnName("seller_role_snapshot")
            .HasMaxLength(80)
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