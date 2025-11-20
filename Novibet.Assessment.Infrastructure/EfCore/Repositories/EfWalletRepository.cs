using Microsoft.EntityFrameworkCore;
using Novibet.Assessment.Application.Features.Wallets;
using Novibet.Assessment.Domain.Entities;
using Novibet.Assessment.Infrastructure.Persistence;

namespace Novibet.Assessment.Infrastructure.EfCore.Repositories;

public class EfCoreWalletRepository : IWalletRepository
{
    private readonly NovibetAssessmentDbContext _context;

    public EfCoreWalletRepository(NovibetAssessmentDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
    }

    public async Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await _context.Wallets.AddAsync(wallet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
