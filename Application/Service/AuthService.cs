using Application;
using ASP.NET_Hands_on.Application.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP.NET_Hands_on.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;

        public AuthService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }


        public async Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (username == "admin" && password == "Admin@123")
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
