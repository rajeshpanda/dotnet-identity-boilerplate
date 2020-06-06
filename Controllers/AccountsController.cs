using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clipp.Server.Models.Common;
using Clipp.Server.Models.User;
using Clipp.Server.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clipp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public AccountsController(
            IUsersService usersService
            )
        {
            _usersService = usersService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var result = await _usersService.Login(model);

            if (result == null)
            {
                return BadRequest();
            }

            return Ok(result);

        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [Route("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserDTO model)
        {
            await _usersService.Register(model);
            return Ok();
        }

        [HttpPost]
        [Route("createAdmin")]
        public async Task<IActionResult> CreateAdminAsync([FromBody] UserDTO model)
        {
            await _usersService.Register(model);
            return Ok();
        }

        [HttpPost]
        [Route("resetPassword")]
        public async Task<IActionResult> ResetPassworAwsync([FromBody] ResetPasswordDTO model)
        {
            await _usersService.ResetPassword(model);
            return Ok();
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenDTO refreshToken)
        {
            return Ok(await _usersService.ValidateRefreshToken(refreshToken.RefreshToken));
        }

        [HttpGet]
        [Route("users")]
        [Authorize(Policy = "ADMIN")]
        public IActionResult GetUsers([FromQuery] BaseRequestDTO request)
        {
            return Ok(_usersService.GetUsers(request, null));
        }

        [HttpGet]
        [Route("users/{id}")]
        [Authorize(Policy = "ADMIN")]
        public IActionResult GetUserById(string id)
        {
            return Ok(_usersService.GetUserById(id));
        }

        [HttpPut]
        [Route("users/{id}")]
        [Authorize(Policy = "ADMIN")]

        public async Task<IActionResult> UpdateUserAsync(string id, [FromBody] UserUpdateDTO request)
        {
            await _usersService.UpdateUserAsync(id, request);
            return Ok();
        }
    }
}