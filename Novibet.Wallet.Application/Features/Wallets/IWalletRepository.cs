using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Application.Features.Wallets;

public interface IWalletRepository
{
    Task<WalletEntity?> GetByIdAsync(long walletId, CancellationToken cancellationToken = default);
    Task<WalletEntity> AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default);
    Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default);
}
