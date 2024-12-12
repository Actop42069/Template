using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p => p.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Email)
                .HasMaxLength(100)
                .IsRequired();
            builder.HasIndex(h => h.Email)
                .IsUnique();

            builder.Property(p => p.UserName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();
            builder.HasIndex(h => h.PhoneNumber)
                .IsUnique();

            builder.Property(p => p.IsActive)
               .IsRequired();

            builder.Property(p => p.LastUpdatedAt)
                .IsRequired();

            builder.Property(p => p.LastUpdatedBy)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(p => p.Token)
                .HasMaxLength(200)
                .IsRequired(false);
        }
    }
}
