namespace Novibet.Assessment.Application.Features.Wallets;

public readonly record struct AdjustBalanceRequest(long WalletId, decimal Amount, string Currency, WalletAdjustmentStrategy Strategy);
