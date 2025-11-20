using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Novibet.Assessment.Application.Features.CurrencyRates;
using Novibet.Assessment.EcbGateway;
using Novibet.Assessment.Infrastructure.BackgroundServices;
using Novibet.Assessment.Infrastructure.BackgroundServices.Hangfire;
using Novibet.Assessment.Infrastructure.EfCore.Repositories;
using Novibet.Assessment.Infrastructure.Options;
using Novibet.Assessment.Infrastructure.Persistence;

namespace Novibet.Assessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastructureSettings settings)
    {
        services.AddEcbGateway();

        if (string.IsNullOrWhiteSpace(settings.SqlServerConnectionString))
        {
            throw new InvalidOperationException("Database connection string is missing");
        }

        services.AddDbContext<NovibetAssessmentDbContext>(options =>
            options.UseSqlServer(settings.SqlServerConnectionString));

        var hangfireOptions = settings.Hangfire ?? new HangfireOptions();

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

        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
        services.AddScoped<IJobRegistration, CurrencyRateHangfireJobs>();

        return services;
    }
}
