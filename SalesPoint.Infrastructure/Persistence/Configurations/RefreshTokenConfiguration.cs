using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(item => item.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(item => item.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(item => item.RevokedAt).HasColumnName("revoked_at");
        builder.Property(item => item.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash").HasMaxLength(64);
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(item => item.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(item => item.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.HasIndex(item => item.TokenHash).IsUnique();
        builder.HasIndex(item => new { item.UserId, item.ExpiresAt });
        builder.HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
