using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<WalletEntity>
{
    public void Configure(EntityTypeBuilder<WalletEntity> builder)
    {
        builder.ToTable("Wallets");

        builder.Property(w => w.Id)
               .ValueGeneratedOnAdd();

        builder.Property(wallet => wallet.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(wallet => wallet.Currency)
            .HasMaxLength(3)
            .IsRequired();
    }
}

