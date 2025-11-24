using Hangfire;
using Novibet.Wallet.Application.Features.CurrencyRates;

namespace Novibet.Wallet.Infrastructure.BackgroundServices.Hangfire;

public class HangfireJobs : IBackgroundJobConfigurator
{
    private readonly IRecurringJobManager _recurringJobManager;
    private const string UpdateCurrencyRatesJob = nameof(UpdateCurrencyRatesJob);
    private const string WarmAvailableCurrenciesJob = nameof(WarmAvailableCurrenciesJob);
    public HangfireJobs(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }
    public void RegisterCurrencyRatesJobs(string cron)
    {
        _recurringJobManager.AddOrUpdate<ICurrencyRateUpdater>(
            UpdateCurrencyRatesJob,
            updater => updater.UpdateRatesAsync(CancellationToken.None),
           cron);


    }

    public void ScheduleAvailableCurrenciesForCache()
    {
        BackgroundJob.Schedule<ICurrencyRateRepository>(
            repo => repo.GetAvailableCurrenciesAsync(CancellationToken.None),
            TimeSpan.FromSeconds(1)
        );
        _recurringJobManager.AddOrUpdate<ICurrencyRateRepository>(
            WarmAvailableCurrenciesJob,
            repo => repo.GetAvailableCurrenciesAsync(CancellationToken.None),
           Cron.Never);
    }

}

