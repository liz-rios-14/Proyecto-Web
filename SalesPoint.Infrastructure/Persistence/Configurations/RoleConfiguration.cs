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
        builder.Property(role => role.Id).HasColumnName("id");
        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(40).IsRequired();

        builder.HasIndex(role => role.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "ADMINISTRATOR" },
            new { Id = 2, Name = "SELLER" });
    }
}
