using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("invoice_details");

        builder.Property<int>("Id")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.Property(detail => detail.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(detail => detail.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(detail => detail.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(detail => detail.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Ignore(detail => detail.Subtotal);
    }
}