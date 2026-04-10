using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login()
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

            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.Sub, "TestSub"),
               new Claim("username", "testUsername123"),
               new Claim("permission", "order.read"),
               new Claim("permission", "order.create"),
               new Claim(ClaimTypes.Role, "Admin"),
               new Claim("tenant", "vn")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
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
