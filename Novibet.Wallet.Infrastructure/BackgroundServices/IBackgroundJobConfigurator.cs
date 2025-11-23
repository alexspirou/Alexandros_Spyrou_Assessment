namespace Novibet.Wallet.Infrastructure.BackgroundServices;

public interface IBackgroundJobConfigurator
{
    void RegisterCurrencyRatesJobs(string cron);
    void ScheduleAvailableCurrenciesForCache();

}

