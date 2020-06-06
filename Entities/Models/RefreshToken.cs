using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Entities.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime Expiry { get; set; }

        public DateTime GeneratedOn { get; set; }

        public bool IsActive { get; set; }

        public string Token { get; set; }
    }
}
