using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;

namespace Novibet.Wallet.Application.Features.Wallets.Requests;

public class CreateWalletRequestValidator : AbstractValidator<CreateWalletRequest>
{
    public CreateWalletRequestValidator(ICurrencyRateRepository currencyRateRepository)
    {
        RuleFor(x => x.Currency)
            .MustBeSupportedCurrency(currencyRateRepository);

        RuleFor(x => x.InitialBalance)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Initial balance cannot be negative.");
    }
}
