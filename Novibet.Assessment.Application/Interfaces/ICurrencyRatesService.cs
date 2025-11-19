using Novibet.Assessment.Application.Responses;

namespace Novibet.Assessment.Application.Interfaces;

public interface ICurrencyRatesService
{
    Task<IReadOnlyList<CurrencyRatesResponse>> GetLatestRates(CancellationToken cancellationToken = default);

}
