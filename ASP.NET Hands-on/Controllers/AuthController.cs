using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
        {
            //// For demonstration, we use hardcoded credentials. In a real application, validate against a database.
            //var username = data.GetValueOrDefault("username", "");
            //var password = data.GetValueOrDefault("password", "");
            //if (username == "admin" && password == "password")
            //{
            //    // Generate a simple token (for demonstration purposes only)
            //    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            //    return Ok(new { Token = token });
            //}
            //return Unauthorized("Invalid username or password.");
            var valid = await _authService.ValidateUserAsync(request.UserName, request.Password, cancellationToken);
            if (!valid)
            {
                throw new AuthenticationException();
            }

            return Ok(new { Token = _authService.IssueJwtAdminAsync(request.UserName) });
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok(new
            {
                UserId = User.FindFirst("sub")?.Value,
                Tenant = User.FindFirst("tenant")?.Value
            });
        }
    }
}
