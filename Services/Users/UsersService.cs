using Clipp.Server.Entities;
using Clipp.Server.Entities.Models;
using Clipp.Server.Models.Common;
using Clipp.Server.Models.User;
using Clipp.Server.Services.Emails;
using LinqKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Clipp.Server.Services.Users
{
    public class UsersService : IUsersService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ClippContext _context;
        private readonly IEmailsService _emailsService;

        public UsersService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ClippContext context,
            IConfiguration configuration,
            IEmailsService emailsService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _emailsService = emailsService;
        }

        public async Task EditUser(UserDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            await AddUserToRolesAsync(user, model.Roles);
        }

        public async Task ForgotPassword(ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // send email with token----------------
            var emailTemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Templates\welcome.html");
            var email = File.ReadAllText(emailTemplatePath);
            var replacements = new Dictionary<string, string>
                {
                    { "FIRST_NAME", "there" },
                    { "SIGNUP_LINK", $"http://localhost:4200/reset-pwd?email={model.Email}&securityToken={HttpUtility.UrlEncode(token)}" }
                };
            email = _emailsService.ReplaceEmail(email, replacements);
            await _emailsService.SendEmailAsync(new List<string> { model.Email }, "unmashedtech@gmail.com", "Account created", email);

            // ----------------------------------------------
            // send email with generate token and email
        }

        public IEnumerable<UserDTO> ListUsers(string role)
        {
            var allUsers = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .Select(c => new UserDTO
                { Id = c.Id, Name = $"{c.Name}", Email = c.Email, Roles = c.UserRoles.Select(x => x.Role.Name).ToList() })
                .AsEnumerable();

            return allUsers.Where(c => c.Roles.Contains(role));
        }

        public async Task<TokenDTO> Login(LoginDTO model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.Include(c => c.UserRoles).ThenInclude(u => u.Role).SingleOrDefault(r => r.Email == model.Email && r.IsActive);
                return new TokenDTO
                {
                    AccessToken = GenerateJwtToken(appUser),
                    RefreshToken = await GetRefreshTokenAsync(appUser)
                };
            }

            return null;
        }

        public async Task Register(UserDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone,
                Name = model.Name,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            await AddUserToRolesAsync(user, model.Roles);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (result.Succeeded && !string.IsNullOrEmpty(token))
            {
                // send email with token----------------
                var emailTemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Templates\welcome.html");
                var email = File.ReadAllText(emailTemplatePath);
                var replacements = new Dictionary<string, string>
                {
                    { "FIRST_NAME", "there" },
                    { "SIGNUP_LINK", $"http://localhost:4200/reset-pwd?email={model.Email}&securityToken={HttpUtility.UrlEncode(token)}" }
                };
                email = _emailsService.ReplaceEmail(email, replacements);
                await _emailsService.SendEmailAsync(new List<string> { model.Email }, "unmashedtech@gmail.com", "Account created", email);

                // ----------------------------------------------
                return;
            }

            throw new Exception();
        }

        public async Task ChangePassword(ChangePasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new Exception();
            }
        }

        public async Task ResetPassword(ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _userManager.ResetPasswordAsync(user, model.SecurityToken, model.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception();
            }
        }

        private string GenerateJwtToken(ApplicationUser user, bool isRefreshToken = false)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.Name)
            };

            var roles = user.UserRoles.Select(c => c.Role.NormalizedName);

            foreach (var role in roles)
            {
                claims.Add(new Claim("groups", role));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(Convert.ToDouble(isRefreshToken ? "24000" : _configuration["JwtExpireHours"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task AddUserToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            await _userManager.AddToRolesAsync(user, roles);
        }

        private async Task RemoveUserFromRolesAsync(ApplicationUser user, List<string> roles)
        {
            await _userManager.RemoveFromRolesAsync(user, roles);
        }

        private async Task<string> GetRefreshTokenAsync(ApplicationUser user)
        {
            await InvalidateRefreshToken(user);
            var rt = GenerateJwtToken(user, true);
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Expiry = DateTime.Now.AddDays(1000),
                IsActive = true,
                GeneratedOn = DateTime.Now,
                Token = rt,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            return rt;
        }

        private async Task InvalidateRefreshToken(ApplicationUser user)
        {
            var existingRT = await _context.RefreshTokens.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);

            if (existingRT != null)
            {
                existingRT.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TokenDTO> ValidateRefreshToken(string token)
        {
            bool isNeedNewRT = false;
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(c => c.Token == token && c.IsActive);
            var user = _userManager.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).SingleOrDefault(r => r.Id == refreshToken.UserId && r.IsActive);
            if (refreshToken == null || user == null)
            {
                return null;
            }

            if (DateTime.Today.Subtract(refreshToken.GeneratedOn).TotalDays > 30)
            {
                isNeedNewRT = true;
                await InvalidateRefreshToken(user);
            }

            return new TokenDTO
            {
                AccessToken = GenerateJwtToken(user),
                RefreshToken = isNeedNewRT ? await GetRefreshTokenAsync(user) : token
            };
        }

        public ICollection<UserResponseDTO> GetUsers(BaseRequestDTO request, string email)
        {
            var predicate = PredicateBuilder.New<ApplicationUser>(true);
            if (!string.IsNullOrEmpty(request.Search))
            {
                predicate.And(c => c.Name.Contains(request.Search, StringComparison.InvariantCultureIgnoreCase)
                    || c.Email.Contains(request.Search, StringComparison.InvariantCultureIgnoreCase));
            }
            if (!string.IsNullOrEmpty(email))
            {
                predicate.And(c => c.Email != email);
            }
            return _context.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).Where(predicate.Compile()).OrderBy(c => c.Name).Select(c => new UserResponseDTO
            {
                Email = c.Email,
                Id = c.Id,
                Name = c.Name,
                Status = c.IsActive ? "Active" : "Disabled",
                Roles = string.Join(",", c.UserRoles.Select(w => w.Role.NormalizedName).ToList())
            }).ToList();
        }

        public UserResponseDTO GetUserById(string id)
        {
            var predicate = PredicateBuilder.New<ApplicationUser>(true);
            if (!string.IsNullOrEmpty(id))
            {
                predicate.And(c => c.Id == id);
            }
            var user = _context.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).FirstOrDefault(predicate.Compile());

            if (user == null)
            {
                return null;
            }
            return new UserResponseDTO
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                Status = user.IsActive ? "Active" : "Disabled",
                Roles = string.Join(",", user.UserRoles.Select(w => w.Role.NormalizedName).ToList())
            };
        }

        public async Task UpdateUserAsync(string id, UserUpdateDTO request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new Exception();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            await this.RemoveUserFromRolesAsync(user, userRoles.ToList());


            await this.AddUserToRolesAsync(user, request.Roles);

            user = await _context.Users.FirstOrDefaultAsync(c => c.Id == id);
            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
        }
    }
}
