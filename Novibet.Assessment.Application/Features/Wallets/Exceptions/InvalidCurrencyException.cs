using System.Net;

namespace Novibet.Assessment.Application.Features.Wallets.Exceptions;

public class InvalidCurrencyException : BaseWalletException
{
    public InvalidCurrencyException(string paramName)
        : base($"Currency value is required for parameter '{paramName}'.", HttpStatusCode.BadRequest) { }
}
