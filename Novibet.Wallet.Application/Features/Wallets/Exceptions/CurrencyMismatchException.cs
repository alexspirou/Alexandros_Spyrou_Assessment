using System.Net;

namespace Novibet.Wallet.Application.Features.Wallets.Exceptions;

public class CurrencyMismatchException : BaseWalletException
{
    public CurrencyMismatchException(string walletCurrency, string requestedCurrency)
        : base($"Wallet currency '{walletCurrency}' does not match requested currency '{requestedCurrency}'. Conversion is not supported.", HttpStatusCode.BadRequest) { }
}

