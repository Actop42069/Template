using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasOne(h => h.User)
                .WithMany(w => w.UserRoles)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.Role)
                .WithMany(w => w.RoleUsers)
                .HasForeignKey(h => h.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
