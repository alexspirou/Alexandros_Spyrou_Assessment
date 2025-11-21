using System.Net;

namespace Novibet.Wallet.Application.Features.Wallets.Exceptions;

public abstract class BaseWalletException : Exception
{
    protected BaseWalletException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}

