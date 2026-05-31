using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("sale_details");

        builder.Property<int>("Id").HasColumnName("id").ValueGeneratedOnAdd();
        builder.HasKey("Id");

        builder.Property(detail => detail.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(detail => detail.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(detail => detail.ProductName).HasColumnName("product_name").HasMaxLength(120).IsRequired();
        builder.Property(detail => detail.Price).HasColumnName("price").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(detail => detail.Quantity).HasColumnName("quantity").IsRequired();

        builder.HasOne(detail => detail.Sale)
            .WithMany(sale => sale.Details)
            .HasForeignKey(detail => detail.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(detail => detail.Product)
            .WithMany()
            .HasForeignKey(detail => detail.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(detail => detail.Subtotal);
    }
}
