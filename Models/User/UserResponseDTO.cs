using System;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Models.User
{
    public class UserResponseDTO
    {
        public string Email { get; set; }

        public string Roles { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string Status { get; set; }
    }
}
