namespace Novibet.Assessment.Application.Responses;

public record CurrencyRatesResponse(
    string Currency,
    decimal Rate,
    DateTime Date
);