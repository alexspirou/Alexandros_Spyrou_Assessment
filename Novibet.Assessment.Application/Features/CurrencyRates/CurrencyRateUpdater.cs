
namespace Novibet.Assessment.Application.Features.CurrencyRates;

internal class CurrencyRateUpdater : ICurrencyRateUpdater
{
    private readonly ICurrencyRatesService _currencyRatesService;
    private readonly ICurrencyRateRepository _currencyRateRepository;
    public CurrencyRateUpdater(ICurrencyRatesService currencyRatesService, ICurrencyRateRepository currencyRateRepository)
    {
        _currencyRatesService = currencyRatesService;
        _currencyRateRepository = currencyRateRepository;
    }
    public async Task<CurrencyRatesSaveResult> UpdateRatesAsync(CancellationToken cancellationToken)
    {

        var rates = await _currencyRatesService.GetLatestRatesAsync(cancellationToken);

        // TODO: break cache here only in case that sql rows affected
        return await _currencyRateRepository.SaveCurrencyRatesAsync(rates, cancellationToken);
    }
}
