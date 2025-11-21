using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Application.Features.Wallets;

public interface IWalletService
{
    Task<WalletEntity> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(long walletId, string? currency = null, CancellationToken cancellationToken = default);
    Task<WalletEntity> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default);
}

