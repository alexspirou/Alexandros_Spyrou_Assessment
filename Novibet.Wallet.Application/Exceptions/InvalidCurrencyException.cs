using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public class InvalidCurrencyException : BaseWalletException
{
    public InvalidCurrencyException(string paramName)
        : base($"Currency value is required for parameter '{paramName}'.", HttpStatusCode.BadRequest) { }
}

