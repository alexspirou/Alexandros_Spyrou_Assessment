using Microsoft.Extensions.DependencyInjection;
using Novibet.Wallet.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace Novibet.Wallet.Infrastructure.Tests;


public sealed class DbContextWithScope : IAsyncDisposable
{
    public IServiceScope Scope { get; }
    public NovibetWalletDbContext DbContext { get; }

    public DbContextWithScope(IServiceScope scope, NovibetWalletDbContext dbContext)
    {
        Scope = scope;
        DbContext = dbContext;
    }

    public async ValueTask DisposeAsync()
    {
        if (Scope is IAsyncDisposable ad) await ad.DisposeAsync();
        else Scope.Dispose();
    }
}
