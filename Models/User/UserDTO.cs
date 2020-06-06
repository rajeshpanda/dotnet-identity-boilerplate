using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Models.User
{
    public class UserDTO
    {
        public string Id { get; set; }

        [Required]
        public string Email { get; set; }

        public ICollection<string> Roles { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }

        // enable in V2
        // public bool IsLockedOut { get; set; }
    }
}
