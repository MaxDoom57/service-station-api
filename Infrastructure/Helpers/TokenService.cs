using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <inheritdoc/>
        public Task<string> GenerateToken(UsrMas user, int projectKey)
        {
            var claims = new[]
            {
                new Claim("UsrKy", user.UsrKy.ToString()),
                new Claim("UsrId", user.UsrId),
                new Claim("CKy", user.CKy.ToString()),
                new Claim("PrjKy", projectKey.ToString())
            };

            return GenerateTokenFromClaims(claims);
        }

        /// <inheritdoc/>
        public Task<string> GenerateTokenFromClaims(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            // Filter out standard JWT claims that would conflict (iss, aud, exp, iat, nbf)
            // so we only carry the application-level claims forward.
            var standardClaimTypes = new HashSet<string>
            {
                JwtRegisteredClaimNames.Iss,
                JwtRegisteredClaimNames.Aud,
                JwtRegisteredClaimNames.Exp,
                JwtRegisteredClaimNames.Iat,
                JwtRegisteredClaimNames.Nbf,
                JwtRegisteredClaimNames.Jti
            };

            var filteredClaims = claims
                .Where(c => !standardClaimTypes.Contains(c.Type))
                .ToList();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(filteredClaims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }
    }
}

