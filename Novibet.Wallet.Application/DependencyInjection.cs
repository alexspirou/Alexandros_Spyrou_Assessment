using Microsoft.Extensions.DependencyInjection;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets;

namespace Novibet.Wallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddSingleton(_ => TimeProvider.System);

        return services;
    }
}

