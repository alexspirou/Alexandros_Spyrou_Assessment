using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Application.Features.Wallets;

public interface IWalletService
{
    Task<long> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(long walletId, string? toCurrency = null, CancellationToken cancellationToken = default);
    Task<WalletEntity> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default);
}

