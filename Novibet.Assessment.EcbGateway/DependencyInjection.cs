using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Novibet.Assessment.Application.Features.CurrencyRates;
using Polly;

namespace Novibet.Assessment.EcbGateway;

public static class DependencyInjection
{
    public static IServiceCollection AddEcbGateway(this IServiceCollection services)
    {
        services
            .AddHttpClient<ICurrencyRatesService, EcbCurrencyRatesService>()
            .AddResilienceHandler("ecb-gateway", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(3),

                    //ShouldHandle = args =>
                    //{
                    //    // Retry on ANY exception
                    //    if (args.Exception is not null)
                    //        return ValueTask.FromResult(true);

                    //    return ValueTask.FromResult(false);
                    //}
                });
            });

        return services;
    }

}
