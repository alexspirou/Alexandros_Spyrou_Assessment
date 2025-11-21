namespace Novibet.Wallet.Domain.Entities;

public class WalletEntity
{
    public long Id { get; set; }
    public decimal Balance { get; set; }
    public required string Currency { get; set; } = string.Empty;
}
