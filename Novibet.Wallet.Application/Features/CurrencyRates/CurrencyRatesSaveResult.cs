namespace Novibet.Wallet.Application.Features.CurrencyRates;

public sealed record CurrencyRatesSaveResult(int AffectedRows)
{
    public bool HasChanges => AffectedRows > 0;
}

