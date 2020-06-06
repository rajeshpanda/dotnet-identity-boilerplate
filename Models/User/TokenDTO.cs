using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Models.User
{
    public class TokenDTO
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string ExpiresIn { get; set; } = "3600";
    }
}
