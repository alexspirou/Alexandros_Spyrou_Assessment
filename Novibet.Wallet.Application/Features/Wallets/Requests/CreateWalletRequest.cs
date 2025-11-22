namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public record CreateWalletRequest(string Currency, decimal InitialBalance = 0);
