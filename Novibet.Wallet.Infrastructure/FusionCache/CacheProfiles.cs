using Novibet.Wallet.Application.Cache;
using ZiggyCreatures.Caching.Fusion;

namespace Novibet.Wallet.Infrastructure.FusionCache;

public static class CacheProfiles
{
    public static FusionCacheEntryOptions Get(CacheProfile profile, TimeSpan? duration = null)
    {
        var options = new FusionCacheEntryOptions();

        switch (profile)
        {
            case CacheProfile.MemoryOnly:
                options.SetSkipMemoryCacheRead(false)
                    .SetSkipMemoryCacheWrite(false)
                    .SetSkipDistributedCache(true, true);
                break;
            case CacheProfile.DistributedOnly:
                options.SetSkipMemoryCacheRead(true)
                    .SetSkipMemoryCacheWrite(true)
                    .SetSkipDistributedCache(false, true);
                break;
            default:
                options.SetSkipMemoryCacheRead(false)
                    .SetSkipMemoryCacheWrite(false)
                    .SetSkipDistributedCache(false, true);
                break;
        }

        options.SetDistributedCacheDuration(duration ?? TimeSpan.FromMinutes(5));
        return options;
    }
}
