using Application.Abstractions;
using Application.Constants;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles user authentication and Google OAuth login.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService, IGoogleCredentialService googleCredentialService, IUserProfileSerivce userProfileService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IGoogleCredentialService _googleService = googleCredentialService;
        private readonly IUserProfileSerivce _userProfileService = userProfileService;

        /// <summary>
        /// Authenticates a user and returns an access token if the credentials are valid.
        /// </summary>
        /// <param name="user">User login request containing email and password.</param>
        /// <returns>Access token if login is successful.</returns>
        /// <response code="200">Login successfully.</response>
        /// <response code="400">Invalid input format (email/password).</response>
        /// <response code="401">Invalid email or password.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReq user)
        {
            var accessToken = await _authService.Login(user);
            return Ok(new
            {
                AccessToken = accessToken
            });
        }

    }
}
