using Hangfire;
using Novibet.Assessment.Application.Features.CurrencyRates;

namespace Novibet.Assessment.Infrastructure.BackgroundServices.Hangfire;

public class CurrencyRateHangfireJobs : IJobRegistration
{
    private readonly IRecurringJobManager _recurringJobManager;
    private const string UpdateCurrencyRatesJob = nameof(UpdateCurrencyRatesJob);
    public CurrencyRateHangfireJobs(IRecurringJobManager recurringJobManager)
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
}
