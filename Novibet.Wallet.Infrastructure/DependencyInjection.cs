using EcbGateway;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Novibet.Wallet.Application.Cache;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets;
using Novibet.Wallet.Infrastructure.BackgroundServices;
using Novibet.Wallet.Infrastructure.BackgroundServices.Hangfire;
using Novibet.Wallet.Infrastructure.EfCore.Repositories;
using Novibet.Wallet.Infrastructure.FusionCache;
using Novibet.Wallet.Infrastructure.Options;
using Novibet.Wallet.Infrastructure.Persistence;

namespace Novibet.Wallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastructureSettings infraSettings)
    {
        services.AddEcbGateway();

        if (string.IsNullOrWhiteSpace(infraSettings.SqlServerConnectionString))
        {
            throw new InvalidOperationException("Database connection string is missing");
        }

        services.AddDbContext<NovibetWalletDbContext>(options =>
            options.UseSqlServer(infraSettings.SqlServerConnectionString));

        var hangfireOptions = infraSettings.Hangfire;

        if (hangfireOptions is not null && infraSettings.BackgroundServiceEnabled)
        {
            AddHangFire(services, infraSettings, hangfireOptions);
            services.AddScoped<IBackgroundJobConfigurator, HangfireJobs>();

        }

        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>()
                .Decorate<ICurrencyRateRepository, CurrencyRateRepositoryWithCache>();

        services.AddScoped<IWalletRepository, EfWalletRepository>();
        services.AddScoped<IAppCache, FusionCacheService>();

        return services;
    }

    private static void AddHangFire(IServiceCollection services, InfrastructureSettings settings, HangfireOptions hangfireOptions)
    {
        services.AddHangfire((configuration, hangfireConfig) =>
        {
            switch (hangfireOptions.JobStorageMode)
            {
                case JobStorageMode.InMemory:
                    hangfireConfig.UseMemoryStorage();
                    break;
                case JobStorageMode.Sql:
                    hangfireConfig.UseSqlServerStorage(settings.SqlServerConnectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hangfireOptions.JobStorageMode), hangfireOptions.JobStorageMode, "Unsupported Hangfire storage mode.");
            }
        });
        services.AddHangfireServer(options =>
        {
            options.ServerName = string.IsNullOrWhiteSpace(hangfireOptions.ServerName)
                ? "hangfire-server"
                : hangfireOptions.ServerName;
        });
    }
}

