using Microsoft.EntityFrameworkCore;
using Novibet.Wallet.Domain.Entities;
using Novibet.Wallet.Infrastructure.Persistence.Configurations;


namespace Novibet.Wallet.Infrastructure.Persistence;

public class NovibetWalletDbContext(DbContextOptions<NovibetWalletDbContext> options)
    : DbContext(options)
{
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<WalletEntity> Wallets => Set<WalletEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("wl");
        modelBuilder.ApplyConfiguration(new CurrencyRateConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
    }
}
