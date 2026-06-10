using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.UserName).HasColumnName("user_name").HasMaxLength(80).IsRequired();
        builder.Property(item => item.Action).HasColumnName("action").HasMaxLength(80).IsRequired();
        builder.Property(item => item.EntityName).HasColumnName("entity_name").HasMaxLength(80).IsRequired();
        builder.Property(item => item.EntityId).HasColumnName("entity_id").HasMaxLength(80);
        builder.Property(item => item.OldValues).HasColumnName("old_values");
        builder.Property(item => item.NewValues).HasColumnName("new_values");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(item => item.IpAddress).HasColumnName("ip_address").HasMaxLength(64).IsRequired();
        builder.Property(item => item.Path).HasColumnName("path").HasMaxLength(300).IsRequired();
        builder.Property(item => item.HttpMethod).HasColumnName("http_method").HasMaxLength(10).IsRequired();
        builder.Property(item => item.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(item => item.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.HasIndex(item => item.CreatedAt);
        builder.HasIndex(item => new { item.UserName, item.Action, item.EntityName });
    }
}
