using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public class UnsupportedWalletStrategyException : BaseWalletException
{
    public UnsupportedWalletStrategyException(string strategy)
        : base($"Unsupported balance adjustment strategy '{strategy}'.", HttpStatusCode.BadRequest) { }
}

