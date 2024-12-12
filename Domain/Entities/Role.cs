using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Role : IdentityRole
    {
        public int Priority { get; set; }
        public virtual ICollection<UserRole> RoleUsers { get; set; }
    }
}
