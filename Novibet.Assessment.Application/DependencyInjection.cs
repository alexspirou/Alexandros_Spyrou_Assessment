using Microsoft.Extensions.DependencyInjection;
using Novibet.Assessment.Application.Features.CurrencyRates;

namespace Novibet.Assessment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
        services.AddSingleton(_ => TimeProvider.System);

        return services;
    }
}
