using Application;
using ASP.NET_Hands_on.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP.NET_Hands_on.Service
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(ILogger<ProductService> logger, JwtSettings jwtSettings)
        {
            _logger = logger;
            _jwtSettings = jwtSettings;
        }


        public async Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (username == "Admin" && password == "Admin@123")
            {
                return true;
            }
            return false;
        }

        //async if fetch claims from db
        public string IssueJwtAdminAsync(string username)
        {
            // For demonstration, we use hardcoded claims. In a real application, these would be based on the authenticated user's data
            // The function is async of not will depend on the fetching data here
            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.Sub, "TestSub"),
               new Claim("username", username),
               new Claim("permission", "order.read"),
               new Claim("permission", "order.create"),
               new Claim(ClaimTypes.Role, "Admin"),
               new Claim("tenant", "vn")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
