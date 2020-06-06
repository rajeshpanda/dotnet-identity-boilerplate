using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Entities.Models
{
    public class ApplicationUser: IdentityUser
    {
        public bool IsActive { get; set; }

        public string Name { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
