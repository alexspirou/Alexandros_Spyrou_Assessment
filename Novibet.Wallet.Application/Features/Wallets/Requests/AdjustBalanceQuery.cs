namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public record AdjustBalanceQuery(
    decimal Amount,
    string Currency,
    WalletAdjustmentStrategy Strategy);
