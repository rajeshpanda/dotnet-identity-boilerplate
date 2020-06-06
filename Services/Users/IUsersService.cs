using Clipp.Server.Models.Common;
using Clipp.Server.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Services.Users
{
    public interface IUsersService
    {
        Task<TokenDTO> Login(LoginDTO model);

        Task Register(UserDTO model);

        Task ForgotPassword(ForgotPasswordDTO model);

        Task ResetPassword(ResetPasswordDTO model);

        Task EditUser(UserDTO model);

        IEnumerable<UserDTO> ListUsers(string role);

        Task<TokenDTO> ValidateRefreshToken(string token);

        ICollection<UserResponseDTO> GetUsers(BaseRequestDTO request, string email);

        UserResponseDTO GetUserById(string id);

        Task UpdateUserAsync(string id, UserUpdateDTO request);
    }
}
