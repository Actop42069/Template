using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class DbInitializer
    {
        private readonly ModelBuilder _modelBuilder;
        public DbInitializer(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public void SeedUserAndRole()
        {
            var dbRoleSuperAdmin = new Role { Id = "1d2a6ea6-0776-43e8-b41f-8fd71c50271a", Name = "Super Admin", NormalizedName = "SUPER ADMIN", Priority = 1, ConcurrencyStamp = "864bfd49-b615-44e3-8913-8e8a1828d3dc" };
            var dbRoleAdmin = new Role { Id = "3f3c9d8e-3f3e-441a-82d8-0290a3744d0d", Name = "Admin", NormalizedName = "Admin", Priority = 2, ConcurrencyStamp = "3553e99a-3949-46fb-9db7-68de70e7ad63" };
            var dbRoleUser = new Role { Id = "ae888c3b-d184-4562-a2b7-97e9e67e81c9", Name = "User", NormalizedName = "USER", Priority = 2, ConcurrencyStamp = "0bb65ffc-d232-4598-b0c8-b536d764fb15" };

            _modelBuilder.Entity<Role>().HasData(dbRoleSuperAdmin, dbRoleAdmin, dbRoleUser);

            var dbUser = new User
            {
                Id = "b022ddbd-03b2-4afd-a100-e345d23e62c0",
                FirstName = "Achyut",
                LastName = "Gaihre",
                Email = "achyut@techglazers.com",
                NormalizedEmail = "ACHYUT@TECHGLAZERS.COM",
                UserName = "actop",
                NormalizedUserName = "ACTOP",
                EmailConfirmed = true,
                IsActive = true,
                PhoneNumber = "9000000000",
                ConcurrencyStamp = "46cc681c-42d9-4b39-b7f5-dade9544c05a",
                LastUpdatedAt = new DateTimeOffset(new DateTime(2022, 12, 31, 5, 49, 5, 819, DateTimeKind.Unspecified).AddTicks(5132), new TimeSpan(0, 0, 0, 0, 0)),
                SecurityStamp = "9a31a1a1-c420-4a4f-b16d-f238687e01f2"
            };
            _modelBuilder.Entity<User>().HasData(dbUser);

            var dbUserRole = new UserRole { UserId = dbUser.Id, RoleId = dbRoleSuperAdmin.Id };
            _modelBuilder.Entity<UserRole>().HasData(dbUserRole);
        }
    }
}
