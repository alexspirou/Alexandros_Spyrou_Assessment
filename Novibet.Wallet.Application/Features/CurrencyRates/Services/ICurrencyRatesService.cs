using Novibet.Wallet.Application.Features.CurrencyRates.Responses;

namespace Novibet.Wallet.Application.Features.CurrencyRates.Services;

public interface ICurrencyRatesService
{
    Task<IReadOnlyList<CurrencyRatesResponse>> GetLatestRatesAsync(CancellationToken cancellationToken = default);

}

