using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(paymentMethod => paymentMethod.Id);
        builder.Property(paymentMethod => paymentMethod.Id).HasColumnName("id");
        builder.Property(paymentMethod => paymentMethod.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(paymentMethod => paymentMethod.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(paymentMethod => paymentMethod.Name).IsUnique();

        builder.HasData(new { Id = 1, Name = "CASH", IsActive = true });
    }
}
