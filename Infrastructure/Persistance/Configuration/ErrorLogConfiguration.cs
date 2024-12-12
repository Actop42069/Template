using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
    {
        public void Configure(EntityTypeBuilder<ErrorLog> builder)
        {

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Timestamp)
                .IsRequired();

            builder.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.StackTrace)
                .HasMaxLength(1000);

            builder.Property(e => e.ErrorType)
                .HasMaxLength(200);
        }
    }
}