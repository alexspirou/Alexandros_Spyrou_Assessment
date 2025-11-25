using Novibet.Wallet.Application.Cache;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;
using Novibet.Wallet.Application.Features.CurrencyRates.Responses;

namespace Novibet.Wallet.Application.Features.CurrencyRates;

public class CurrencyRateRepositoryWithCache : ICurrencyRateRepository
{
    private readonly ICurrencyRateRepository _inner;
    private readonly IAppCache _cache;
    private readonly TimeProvider _timeProvider;

    public CurrencyRateRepositoryWithCache(ICurrencyRateRepository inner, IAppCache cache, TimeProvider timeProvider)
    {
        _inner = inner;
        _cache = cache;
        _timeProvider = timeProvider;
    }
    public async Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        const string key = "cr:available";

        var cacheOptions = new CacheOptions(Duration: TimeSpan.MaxValue);
        var result = await _cache.GetOrSetAsync(
            key,
            _ => _inner.GetAvailableCurrenciesAsync(cancellationToken),
            cacheOptions,
            cancellationToken);

        return result!;
    }

    public async Task<IDictionary<string, decimal>> GetCurrencyRates(IEnumerable<string> currencyCodes, DateOnly date, CancellationToken cancellationToken = default)
    {
        var codes = string.Join('-', currencyCodes.OrderBy(c => c));
        var key = $"cr:{date:yyyyMMdd}:{codes}";

        var result = await _cache.GetOrSetAsync(
            key,
            _ => _inner.GetCurrencyRates(currencyCodes, date, cancellationToken),
            new CacheOptions(Duration: TimeSpan.FromMinutes(1), Profile: CacheProfile.DistributedOnly),
            cancellationToken);

        return result!;
    }


    public Task<CurrencyRatesSaveResult> SaveCurrencyRatesAsync(IReadOnlyList<CurrencyRatesResponse> currencyRates, CancellationToken cancellationToken = default)
    {
        return _inner.SaveCurrencyRatesAsync(currencyRates, cancellationToken);
    }
}
