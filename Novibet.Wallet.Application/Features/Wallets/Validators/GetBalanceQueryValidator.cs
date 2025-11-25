using FluentValidation;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;
using Novibet.Wallet.Application.Features.Wallets.Requests;

namespace Novibet.Wallet.Application.Features.Wallets.Validators;

public class GetBalanceQueryValidator : AbstractValidator<GetBalanceQuery>
{
    public GetBalanceQueryValidator(ICurrencyRateRepository currencyRateRepository)
    {
        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .MustBeSupportedCurrency(currencyRateRepository, true);

    }
}
