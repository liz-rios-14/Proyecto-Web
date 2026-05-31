using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).HasColumnName("id");
        builder.Property(movement => movement.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(movement => movement.SaleId).HasColumnName("sale_id");
        builder.Property(movement => movement.InvoiceId).HasColumnName("invoice_id");
        builder.Property(movement => movement.MovementType).HasColumnName("movement_type").HasMaxLength(40).IsRequired();
        builder.Property(movement => movement.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(movement => movement.StockAfter).HasColumnName("stock_after").IsRequired();
        builder.Property(movement => movement.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(movement => movement.Reason).HasColumnName("reason").HasMaxLength(160).IsRequired();

        builder.HasOne(movement => movement.Product)
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movement => movement.Sale)
            .WithMany()
            .HasForeignKey(movement => movement.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(movement => movement.ProductId);
        builder.HasIndex(movement => movement.CreatedAt);
    }
}
