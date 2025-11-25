using Novibet.Wallet.Application.Features.CurrencyRates.Responses;

namespace Novibet.Wallet.Application.Features.CurrencyRates.Repositories;

public interface ICurrencyRateRepository
{
    Task<CurrencyRatesSaveResult> SaveCurrencyRatesAsync(IReadOnlyList<CurrencyRatesResponse> currencyRates, CancellationToken cancellationToken = default);
    Task<IDictionary<string, decimal>> GetCurrencyRates(IEnumerable<string> currencyCodes, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(CancellationToken cancellationToken = default);
}

