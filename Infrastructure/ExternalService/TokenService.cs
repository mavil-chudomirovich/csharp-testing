using Application.Abstractions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Helpers;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalService
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public TokenService(IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor
            , IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _contextAccessor = httpContextAccessor;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public string GenerateAccessToken(Guid userId)
        {
            return JwtHelper.GenerateUserIDToken(userId, _jwtSettings.AccessTokenSecret, TokenType.AccessToken.ToString(), _jwtSettings.AccessTokenExpiredTime, _jwtSettings.Issuer, _jwtSettings.Audience, null);
        }

        public async Task<string> GenerateRefreshToken(Guid userId, ClaimsPrincipal? oldClaims)
        {
            var _context = _contextAccessor.HttpContext;
            string token = JwtHelper.GenerateUserIDToken(userId, _jwtSettings.RefreshTokenSecret, TokenType.RefreshToken.ToString(),
                _jwtSettings.RefreshTokenExpiredTime, _jwtSettings.Issuer, _jwtSettings.Audience, oldClaims);
            ClaimsPrincipal claims = JwtHelper.VerifyToken(token, _jwtSettings.RefreshTokenSecret, TokenType.RefreshToken.ToString(),
                _jwtSettings.Issuer, _jwtSettings.Audience);
            long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Iat)?.Value, out long iatSeconds);
            long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out long expSeconds);

            await _refreshTokenRepository.AddAsync(new RefreshToken()
            {
                UserId = userId,
                Token = token,
                IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime,
                IsRevoked = false
            });
            //lưu vào cookie
            if (_context != null)
            {
                _context.Response.Cookies.Append(CookieKeys.RefreshToken, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,    //BẮT BUỘC CHO CROSS-DOMAIN
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiredTime), // hạn sử dụng
                    //Domain = ".greenwheel.site",     //để FE gửi cookie vào BE
                    Path = "/"
                });
            }
            return token;
        }
    }
}
