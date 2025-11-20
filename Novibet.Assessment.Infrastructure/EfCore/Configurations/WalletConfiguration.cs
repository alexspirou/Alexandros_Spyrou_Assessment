using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novibet.Assessment.Domain.Entities;

namespace Novibet.Assessment.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(wallet => wallet.Currency)
            .HasMaxLength(3)
            .IsRequired();
    }
}
