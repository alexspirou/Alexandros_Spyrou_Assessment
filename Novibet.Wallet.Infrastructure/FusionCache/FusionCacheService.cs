using Novibet.Wallet.Application.Cache;
using ZiggyCreatures.Caching.Fusion;

namespace Novibet.Wallet.Infrastructure.FusionCache;

public class FusionCacheService : IAppCache
{
    private readonly IFusionCache _fusionCache;

    public FusionCacheService(IFusionCache fusionCache)
    {
        _fusionCache = fusionCache;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CacheOptions options, CancellationToken cancellationToken = default)
    {
        var entryOptions = CacheProfiles.Get(options.Profile, options.Duration);
        return await _fusionCache.GetOrSetAsync(key, f => factory(f), entryOptions, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken = default)
    {
        var entryOptions = CacheProfiles.Get(options.Profile, options.Duration);
        await _fusionCache.SetAsync(key, value, entryOptions, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _fusionCache.RemoveAsync(key, token: cancellationToken);
    }
}
