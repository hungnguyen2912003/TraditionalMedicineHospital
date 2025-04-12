using Microsoft.IdentityModel.Tokens;
using Project.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.Services.Features
{
    public class JwtManager
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secretKey;
        public JwtManager(IConfiguration configuration)
        {
            _issuer = configuration["JwtSettings:Issuer"] ?? throw new ArgumentNullException(nameof(configuration), "Issuer is missing in configuration.");
            _audience = configuration["JwtSettings:Audience"] ?? throw new ArgumentNullException(nameof(configuration), "Audience is missing in configuration.");
            _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new ArgumentNullException(nameof(configuration), "SecretKey is missing in configuration.");
        }

        public string GenerateToken(string username, RoleType role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string? Username, string? Role) GetClaimsFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var username = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
                var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value;
                return (username, role);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
