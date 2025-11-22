using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets;
using System.Linq;

namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public class AdjustBalanceQueryValidator : AbstractValidator<AdjustBalanceQuery>
{
    public AdjustBalanceQueryValidator(ICurrencyRateRepository currencyRateRepository)
    {
        RuleFor(x => x.Amount)
            .NotEqual(0m)
            .WithMessage("Amount must be non-zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter code.")
            .MustAsync(async (currency, ct) =>
            {
                if (string.Equals(currency, CurrencyCodes.Eur, StringComparison.OrdinalIgnoreCase))
                    return true;

                var available = await currencyRateRepository.GetAvailableCurrenciesAsync(ct);
                return available.Any(code => string.Equals(code, currency, StringComparison.OrdinalIgnoreCase));
            })
            .WithMessage("Currency is not supported.");
    }
}
