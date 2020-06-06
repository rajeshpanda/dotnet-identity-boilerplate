using System.Collections.Generic;

namespace Clipp.Server.Models.User
{
    public class UserUpdateDTO
    {
        public List<string> Roles { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public bool IsActive { get; set; }
    }
}
