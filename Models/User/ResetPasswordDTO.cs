namespace Clipp.Server.Models.User
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }

        public string SecurityToken { get; set; }

        public string NewPassword { get; set; }
    }
}
