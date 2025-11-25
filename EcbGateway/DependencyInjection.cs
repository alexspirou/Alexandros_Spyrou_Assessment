using Polly;
using Polly.Retry;

namespace EcbGateway;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Novibet.Wallet.Application.Features.CurrencyRates.Services;
using System.Net.Http;

public static class DependencyInjection
{
    public static IServiceCollection AddEcbGateway(this IServiceCollection services)
    {
        services
            .AddHttpClient<ICurrencyRatesService, EcbCurrencyRatesService>()
            .AddResilienceHandler("ecb-gateway", (builder, context) =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<EcbCurrencyRatesService>>();

                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,

                    ShouldHandle = args =>
                    {
                        var outcome = args.Outcome;

                        if (outcome.Exception is not null)
                        {
                            logger.LogWarning(
                                outcome.Exception,
                                "ECB request failed with exception. Retrying..."
                            );
                            return ValueTask.FromResult(true);
                        }

                        if (outcome.Result is HttpResponseMessage resp &&
                            !resp.IsSuccessStatusCode)
                        {
                            logger.LogWarning(
                                "ECB request failed with HTTP status {Status}. Retrying...",
                                (int)resp.StatusCode
                            );
                            return ValueTask.FromResult(true);
                        }

                        return ValueTask.FromResult(false);
                    }
                });

                builder.AddTimeout(TimeSpan.FromSeconds(30));
            });

        return services;
    }



}

