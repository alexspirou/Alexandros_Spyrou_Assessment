namespace Novibet.Wallet.Application.Features.CurrencyRates.Responses;

public record CurrencyRatesResponse(
    string CurrencyCode,
    decimal Rate,
    DateTime Date
);
