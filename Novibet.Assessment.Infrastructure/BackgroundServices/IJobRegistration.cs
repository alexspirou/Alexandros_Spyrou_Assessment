namespace Novibet.Assessment.Infrastructure.BackgroundServices;

public interface IJobRegistration
{
    void RegisterCurrencyRatesJobs(string cron);

}
