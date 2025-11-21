using Microsoft.EntityFrameworkCore;
using Novibet.Wallet.Application.Features.Wallets;
using Novibet.Wallet.Infrastructure.Persistence;
using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Infrastructure.EfCore.Repositories;

public class EfWalletRepository : IWalletRepository
{
    private readonly NovibetWalletDbContext _context;

    public EfWalletRepository(NovibetWalletDbContext context)
    {
        _context = context;
    }

    public async Task<WalletEntity?> GetByIdAsync(long walletId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
    }

    public async Task<WalletEntity> AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        await _context.Wallets.AddAsync(wallet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

