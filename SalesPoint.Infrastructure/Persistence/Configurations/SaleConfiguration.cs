using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(sale => sale.Id);
        builder.Property(sale => sale.Id).HasColumnName("id");
        builder.Property(sale => sale.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(sale => sale.UserId).HasColumnName("user_id");
        builder.Property(sale => sale.PaymentMethodId).HasColumnName("payment_method_id").IsRequired();
        builder.Property(sale => sale.Date).HasColumnName("date").IsRequired();
        builder.Property(sale => sale.SaleNumber).HasColumnName("sale_number").HasMaxLength(20).IsRequired();
        builder.Property(sale => sale.IsConfirmed).HasColumnName("is_confirmed").IsRequired();

        builder.HasIndex(sale => sale.SaleNumber).IsUnique();

        builder.HasOne(sale => sale.Customer)
            .WithMany()
            .HasForeignKey(sale => sale.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sale => sale.User)
            .WithMany()
            .HasForeignKey(sale => sale.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sale => sale.PaymentMethod)
            .WithMany(paymentMethod => paymentMethod.Sales)
            .HasForeignKey(sale => sale.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(sale => sale.Subtotal);
        builder.Ignore(sale => sale.Tax);
        builder.Ignore(sale => sale.Total);

        builder.Metadata.FindNavigation(nameof(Sale.Details))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
