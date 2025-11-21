namespace Novibet.Wallet.Domain.Entities;

public class CurrencyRate
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string CurrencyCode { get; set; } = string.Empty;
    public required decimal Rate { get; set; }
    public required DateOnly Date { get; set; }
    public required DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

