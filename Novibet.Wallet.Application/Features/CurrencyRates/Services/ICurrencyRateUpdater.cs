namespace Novibet.Wallet.Application.Features.CurrencyRates.Services;

public interface ICurrencyRateUpdater
{
    Task<CurrencyRatesSaveResult> UpdateRatesAsync(CancellationToken cancellationToken);
}

