using Domain.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        /// <summary>Generates a new JWT for the given user and project (used at login).</summary>
        Task<string> GenerateToken(UsrMas user, int projectKey);

        /// <summary>
        /// Re-issues a new JWT carrying the same claims as the supplied colletion,
        /// with a fresh 1-hour expiry. Used by the refresh-token endpoint.
        /// </summary>
        Task<string> GenerateTokenFromClaims(IEnumerable<Claim> claims);
    }
}

