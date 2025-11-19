using Novibet.Assessment.Application.Interfaces;
using Novibet.Assessment.Application.Responses;
using System.Collections.Immutable;
using System.Globalization;
using System.Xml.Linq;

namespace Novibet.Assessment.EcbGateway;

public class EcbCurrencyRatesService : ICurrencyRatesService
{
    //private const string RatesEndpoint = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
    private const string RatesEndpoint = "https://localhost:12345";
    private readonly HttpClient _httpClient;

    public EcbCurrencyRatesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CurrencyRatesResponse>> GetLatestRates(CancellationToken cancellationToken = default)
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
                Currency: x.Attribute("currency")!.Value,
                Rate: decimal.Parse(x.Attribute("rate")!.Value, CultureInfo.InvariantCulture),
                Date: date
            )).ToImmutableList();

        return result;
    }
}
