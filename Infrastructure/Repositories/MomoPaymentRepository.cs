 using Application.AppSettingConfigurations;
using Application.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;


namespace Infrastructure.Repositories
{
    public class MomoPaymentRepository : IMomoPaymentLinkRepository
    {
        private readonly IDistributedCache _cache;
        private readonly MomoSettings _momoSettings;

        public MomoPaymentRepository(IDistributedCache cache, IOptions<MomoSettings> momoSetting)
        {
            _cache = cache;
            _momoSettings = momoSetting.Value;
        }
        public async Task<string?> GetPaymentLinkAsync(string key)
        {
            return await _cache.GetStringAsync($"payment_link:{key}");
        }

        public async Task RemovePaymentLinkAsync(string key)
        {
             await _cache.RemoveAsync($"payment_link:{key}");
        }

        public async Task SavePaymentLinkPAsyns(string key, string link)
        {
            await _cache.SetStringAsync(
                $"payment_link:{key}",
                link,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_momoSettings.Ttl)
                }
            );
        }
    }
}
