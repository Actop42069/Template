using Common.Interfaces;
using Domain.Enumeration;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User : IdentityUser, ICreatedEvent, IUpdatedEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string LastUpdatedBy { get; set; } = "SA";
        public MfaProvider DefaultMfaProvider { get; set; }
        public Gender Gender { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        [NotMapped]
        public string ClientUrl { get; set; }
        [NotMapped]
        public ActivityLog Activity { get; set; }
        public string Token { get; set; }
    }
}
