using System.Net;

namespace Novibet.Wallet.Application.Features.Wallets.Exceptions;

public class InsufficientFundsException : BaseWalletException
{
    public InsufficientFundsException(long walletId, decimal amount, decimal balance)
        : base($"Wallet {walletId} has insufficient funds. Attempted to subtract {amount}, available balance is {balance}.", HttpStatusCode.Conflict) { }
}

