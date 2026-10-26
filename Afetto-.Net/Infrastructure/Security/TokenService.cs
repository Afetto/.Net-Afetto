using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Afetto_.Net.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Afetto_.Net.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public (string Token, DateTime ExpiraEm) GerarToken(Guid usuarioId, string nome, string email)
        {
            var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Name, nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expiraEm);
        }
    }
}
