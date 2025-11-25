using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Application.Features.Wallets.Responses;

namespace Novibet.Wallet.Application.Features.Wallets.Services;

public interface IWalletService
{
    Task<long> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default);
    Task<WalletBalanceResponse> GetBalanceAsync(long walletId, string? toCurrency = null, CancellationToken cancellationToken = default);
    Task<WalletBalanceResponse> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default);
}

