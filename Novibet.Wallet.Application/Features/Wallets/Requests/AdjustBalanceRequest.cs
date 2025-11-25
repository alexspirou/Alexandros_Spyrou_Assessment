namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public readonly record struct AdjustBalanceRequest(long WalletId, decimal Amount, string Currency, WalletAdjustmentStrategy Strategy);

