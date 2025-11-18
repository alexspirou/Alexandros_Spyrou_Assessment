namespace Novibet.Assessment.EcbGateway;

public interface IEcbCurrencyRatesService
{
    public Task Get(CancellationToken cancellationToken);
}
