using Application.AppSettingConfigurations;
using Application.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;

namespace API.Extentions
{
    public static class JwtTokenValidation
    {
        public static void AddJwtTokenValidation(this IServiceCollection services, JwtSettings _jwtSetting)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = _jwtSetting.Issuer,
                        ValidAudience = _jwtSetting.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_jwtSetting.AccessTokenSecret))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Nếu token không tồn tại
                            if (string.IsNullOrEmpty(context.Token))
                            {
                                context.HttpContext.Items["JwtError"] = "MissingToken";
                            }
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var endpoint = context.HttpContext.GetEndpoint();
                            var hasAuthorize = endpoint?.Metadata
                                .GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

                            if (!hasAuthorize)
                                return Task.CompletedTask;

                            // Token tồn tại nhưng sai (expired, signature sai, fake,…)
                            context.HttpContext.Items["JwtError"] = "InvalidAccessToken";
                            return Task.CompletedTask;
                        },

                        OnChallenge = context =>
                        {
                            context.HandleResponse(); // Ngăn ASP.NET trả lỗi mặc định

                            var endpoint = context.HttpContext.GetEndpoint();
                            var hasAuthorize = endpoint?.Metadata
                                .GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

                            if (!hasAuthorize)
                                return Task.CompletedTask;

                            var error = context.HttpContext.Items["JwtError"]?.ToString();

                            if (error == "MissingToken")
                                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);

                            // mặc định → token invalid
                            throw new UnauthorizedAccessException(Message.UserMessage.InvalidAccessToken);
                        }
                    };
                });
        }

    }
}
