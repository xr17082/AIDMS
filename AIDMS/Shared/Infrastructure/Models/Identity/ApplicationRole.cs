using AIDMS.Shared.Infrastructure.Contracts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AIDMS.Shared.Infrastructure.Models.Identity
{
    public class ApplicationRole : IdentityRole, IAuditableEntity<string>
    {
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }

        public ApplicationRole() : base()
        {
            RoleClaims = new HashSet<ApplicationRoleClaim>();
        }

        public ApplicationRole(string roleName, string roleDescription) : base(roleName)
        {
            RoleClaims = new HashSet<ApplicationRoleClaim>();
            Description = roleDescription;
        }
    }
}
