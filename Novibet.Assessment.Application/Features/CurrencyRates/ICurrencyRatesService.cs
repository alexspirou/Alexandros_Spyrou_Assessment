namespace Novibet.Assessment.Application.Features.CurrencyRates;

public interface ICurrencyRatesService
{
    Task<IReadOnlyList<CurrencyRatesResponse>> GetLatestRatesAsync(CancellationToken cancellationToken = default);

}
