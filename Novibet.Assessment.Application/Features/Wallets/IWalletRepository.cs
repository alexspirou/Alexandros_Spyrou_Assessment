using Novibet.Assessment.Domain.Entities;

namespace Novibet.Assessment.Application.Features.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken = default);
    Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
