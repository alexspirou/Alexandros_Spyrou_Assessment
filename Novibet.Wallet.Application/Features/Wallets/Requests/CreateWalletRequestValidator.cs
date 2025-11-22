using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets;

namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public class CreateWalletRequestValidator : AbstractValidator<CreateWalletRequest>
{
    public CreateWalletRequestValidator(ICurrencyRateRepository currencyRateRepository)
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter code.")
            .MustAsync(async (currency, ct) =>
            {
                if (string.Equals(currency, CurrencyCodes.Eur, StringComparison.OrdinalIgnoreCase))
                    return true;
                // TODO: Make it fixed or add cache here
                var availableCurrencies = await currencyRateRepository.GetAvailableCurrenciesAsync(ct);
                return availableCurrencies.Any(code => string.Equals(code, currency, StringComparison.OrdinalIgnoreCase));
            })
            .WithMessage("Currency is not supported.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Initial balance cannot be negative.");
    }
}
