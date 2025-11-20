using Novibet.Assessment.Domain.Entities;

namespace Novibet.Assessment.Application.Features.Wallets;

public interface IWalletService
{
    Task<Wallet> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(long walletId, string? currency = null, CancellationToken cancellationToken = default);
    Task<Wallet> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default);
}
