using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;
using Novibet.Wallet.Application.Features.Wallets;

namespace Novibet.Wallet.Application.Features.CurrencyRates;

public static class CurrencyRuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeSupportedCurrency<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ICurrencyRateRepository currencyRateRepository,
        bool allowNullOrEmpty = false)
    {
        return ruleBuilder
            .MaximumLength(3)
            .WithMessage("Currency must be a 3-letter code.")
            .MustAsync(async (currency, ct) =>
            {
                if (string.IsNullOrWhiteSpace(currency))
                    return allowNullOrEmpty;

                if (string.Equals(currency, CurrencyCodes.Eur, StringComparison.OrdinalIgnoreCase))
                    return true;

                var availableCurrencies = await currencyRateRepository.GetAvailableCurrenciesAsync(ct);
                return availableCurrencies.Any(code => string.Equals(code, currency, StringComparison.OrdinalIgnoreCase));
            })
            .WithMessage("Currency is not supported.");
    }
}
