using System.Net;

namespace Novibet.Wallet.Application.Exceptions;

public class WalletNotFoundException : BaseWalletException
{
    public WalletNotFoundException(long walletId)
        : base($"Wallet {walletId} was not found.", HttpStatusCode.NotFound)
    {
        WalletId = walletId;
    }

    public long WalletId { get; }
}

