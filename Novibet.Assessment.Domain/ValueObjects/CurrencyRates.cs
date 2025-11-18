namespace Novibet.Assessment.Domain.ValueObjects;

public record CurrencyRates(
    string Currency,
    decimal Rate,
    DateTime Date
);