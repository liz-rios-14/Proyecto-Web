using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
        builder.Property(user => user.UserName).HasColumnName("user_name").HasMaxLength(60).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(120).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(250).IsRequired();
        builder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new
        {
            Id = 1,
            RoleId = 1,
            FullName = "ADMINISTRADOR DEL SISTEMA",
            UserName = "admin",
            Email = "admin@salespoint.local",
            PasswordHash = "CHANGE_ME_HASH_ADMIN_123456",
            IsActive = true
        });
    }
}
