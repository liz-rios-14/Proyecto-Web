using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Configurations;

public sealed class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");

        builder.HasKey(error => error.Id);
        builder.Property(error => error.Id).HasColumnName("id");
        builder.Property(error => error.Source).HasColumnName("source").HasMaxLength(120).IsRequired();
        builder.Property(error => error.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
        builder.Property(error => error.StackTrace).HasColumnName("stack_trace");
        builder.Property(error => error.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(error => error.CreatedAt);
    }
}
