using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .HasColumnName("id");

        builder.Property(customer => customer.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(customer => customer.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(customer => customer.Cedula)
            .HasColumnName("cedula")
            .HasMaxLength(10);

        builder.HasIndex(customer => customer.Cedula)
            .IsUnique()
            .HasFilter("[cedula] IS NOT NULL");

        builder.Property(customer => customer.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(customer => customer.Address)
            .HasColumnName("address")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasColumnName("email")
            .HasMaxLength(120)
            .IsRequired();
    }
}
