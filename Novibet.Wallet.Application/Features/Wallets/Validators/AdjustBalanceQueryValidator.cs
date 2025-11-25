using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;

namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public class AdjustBalanceQueryValidator : AbstractValidator<AdjustBalanceQuery>
{
    public AdjustBalanceQueryValidator(ICurrencyRateRepository currencyRateRepository)
    {
        RuleFor(x => x.Amount)
            .NotEqual(0m)
            .WithMessage("Amount must be non-zero.");

        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(3)
            .WithMessage("Currency must be a 3-letter code.")
            .MustBeSupportedCurrency(currencyRateRepository);


    }
}
