namespace Novibet.Wallet.Application.Features.Wallets.Responses;

public record WalletBalanceResponse(long WalletId, decimal Balance, string Currency);
