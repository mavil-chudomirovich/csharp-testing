using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface ITokenService
    {
        string GenerateAccessToken(Guid userId);
        Task<string> GenerateRefreshToken(Guid userId, ClaimsPrincipal? oldClaims);
    }
}
