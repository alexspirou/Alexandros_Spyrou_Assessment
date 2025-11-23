namespace Novibet.Wallet.Application.Cache;
public interface IAppCache
{
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CacheOptions options, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
