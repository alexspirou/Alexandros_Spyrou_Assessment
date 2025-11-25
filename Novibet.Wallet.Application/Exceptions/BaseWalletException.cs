using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public abstract class BaseWalletException : Exception
{
    protected BaseWalletException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}

