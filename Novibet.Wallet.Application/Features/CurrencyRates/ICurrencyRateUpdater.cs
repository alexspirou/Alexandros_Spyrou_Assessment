namespace Novibet.Wallet.Application.Features.CurrencyRates;

public interface ICurrencyRateUpdater
{
    Task<CurrencyRatesSaveResult> UpdateRatesAsync(CancellationToken cancellationToken);
}

