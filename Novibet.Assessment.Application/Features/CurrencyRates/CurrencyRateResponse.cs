namespace Novibet.Assessment.Application.Features.CurrencyRates;

public record CurrencyRatesResponse(
    string CurrencyCode,
    decimal Rate,
    DateTime Date
);