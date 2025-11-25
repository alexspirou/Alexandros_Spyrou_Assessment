using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public sealed class CurrencyRateNotFoundException : BaseWalletException
{
    public CurrencyRateNotFoundException(string currency, DateOnly date)
        : base($"Currency rate for {currency} on {date:yyyy-MM-dd} was not found.", HttpStatusCode.NotFound)
    {
        Currency = currency;
        Date = date;
    }

    public string Currency { get; }
    public DateOnly Date { get; }
}
