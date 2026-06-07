using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_histories");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id).HasColumnName("id");
        builder.Property(history => history.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(history => history.PasswordHash).HasColumnName("password_hash").HasMaxLength(250).IsRequired();
        builder.Property(history => history.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(history => history.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(history => history.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(history => history.User)
            .WithMany()
            .HasForeignKey(history => history.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}