using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasColumnName("id");

        builder.Property(role => role.Name)
            .HasColumnName("name")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasColumnName("description")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(role => role.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(role => role.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(role => role.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.HasData(
            new
            {
                Id = 1,
                Name = "ADMINISTRATOR",
                Description = "Administrador del sistema",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = 2,
                Name = "SELLER",
                Description = "Vendedor del sistema",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}