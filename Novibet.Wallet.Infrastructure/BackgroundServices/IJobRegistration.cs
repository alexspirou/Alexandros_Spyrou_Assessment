namespace Novibet.Wallet.Infrastructure.BackgroundServices;

public interface IJobRegistration
{
    void RegisterCurrencyRatesJobs(string cron);

}

