using Microsoft.Extensions.Logging;
using Novibet.Wallet.Application.Features.CurrencyRates.Responses;
using Novibet.Wallet.Application.Features.CurrencyRates.Services;
using System.Collections.Immutable;
using System.Globalization;
using System.Xml.Linq;

namespace EcbGateway;

public class EcbCurrencyRatesService : ICurrencyRatesService
{
    private const string RatesEndpoint = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
    private readonly HttpClient _httpClient;
    private readonly ILogger<EcbCurrencyRatesService> _logger;

    public EcbCurrencyRatesService(HttpClient httpClient, ILogger<EcbCurrencyRatesService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CurrencyRatesResponse>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Requesting latest currency rates from ECB endpoint: {RatesEndpoint}");
        try
        {
            var xml = await _httpClient.GetStringAsync(RatesEndpoint, cancellationToken);

            var doc = XDocument.Parse(xml);

            XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";
            XNamespace gesmes = "http://www.gesmes.org/xml/2002-08-01";

            var cube = doc
                .Element(gesmes + "Envelope")!
                .Element(ns + "Cube")!
                .Element(ns + "Cube")!;

            var date = DateTime.Parse(cube.Attribute("time")!.Value);

            var result = cube
                .Elements()
                .Select(x => new CurrencyRatesResponse(
                    CurrencyCode: x.Attribute("currency")!.Value,
                    Rate: decimal.Parse(x.Attribute("rate")!.Value, CultureInfo.InvariantCulture),
                    Date: date
                )).ToImmutableList();

            _logger.LogInformation($"ECB returned {result.Count} currency rates for {date}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve latest currency rates from ECB endpoint {RatesEndpoint}");
            throw;
        }
    }
}
