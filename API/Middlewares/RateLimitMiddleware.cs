using Application.AppExceptions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace API.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        // Lưu các limiter theo IP
        private static readonly ConcurrentDictionary<string, TokenBucketRateLimiter> _limiters = new();

        // Cấu hình giới hạn
        private readonly RateLimitSettings _rateLimitSettings;

        public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitSettings> settings)
        {
            _next = next;
            _rateLimitSettings = settings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Lấy hoặc tạo limiter cho IP
            var limiter = _limiters.GetOrAdd(ip, _ => new TokenBucketRateLimiter(
                new TokenBucketRateLimiterOptions
                {
                    TokenLimit = _rateLimitSettings.TokenLimit,
                    TokensPerPeriod = _rateLimitSettings.TokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(_rateLimitSettings.ReplenishmentPeriod),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _rateLimitSettings.QueueLimit,
                    AutoReplenishment = true
                }));

            using var lease = await limiter.AcquireAsync(1);

            if (lease.IsAcquired)
            {
                // Cho phép request đi tiếp
                await _next(context);
            }
            else
            {
                // Khi bị giới hạn
                throw new RateLimitExceededException(Message.CommonMessage.TooManyRequest);
            }
        }
    }
}