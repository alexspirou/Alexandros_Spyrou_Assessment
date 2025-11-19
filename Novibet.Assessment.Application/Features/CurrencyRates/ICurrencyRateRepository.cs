namespace Novibet.Assessment.Application.Features.CurrencyRates;

public interface ICurrencyRateRepository
{
    Task<CurrencyRatesSaveResult> SaveCurrencyRatesAsync(
        IReadOnlyList<CurrencyRatesResponse> currencyRates,
        CancellationToken cancellationToken = default);
}
