using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserService.API.Services
{
    public class TokenManager
    {
        private readonly SymmetricSecurityKey _securityKey;
        private readonly string _issuer;
        private readonly string _audience;


        public TokenManager(SymmetricSecurityKey securityKey, string issuer, string audience)
        {
            _securityKey = securityKey;
            _issuer = issuer;
            _audience = audience;
        }

        public string GenerateAccessToken(List<Claim> claims)
        {
            var accessToken = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(accessToken);
        }

        public string GenerateRefreshToken(List<Claim> claims)
        {
            var refreshToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(refreshToken);
        }
    }
}
