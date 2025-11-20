using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Novibet.Assessment.Application.Features.CurrencyRates;
using Novibet.Assessment.Infrastructure.Persistence;
using System.Text;

namespace Novibet.Assessment.Infrastructure.EfCore.Repositories;

public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly NovibetAssessmentDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CurrencyRateRepository(NovibetAssessmentDbContext context, TimeProvider timeProvider)
    {
        _dbContext = context;
        _timeProvider = timeProvider;
    }

    public async Task<IDictionary<string, decimal>> GetCurrencyRates(IEnumerable<string> currencyCodes,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CurrencyRates
            .Where(x => currencyCodes.Contains(x.CurrencyCode) && x.Date == date)
            .ToDictionaryAsync(x => x.CurrencyCode, x => x.Rate, cancellationToken);
    }

    public async Task<CurrencyRatesSaveResult> SaveCurrencyRatesAsync(
        IReadOnlyList<CurrencyRatesResponse> currencyRates,
        CancellationToken cancellationToken = default)
    {
        if (currencyRates == null || currencyRates.Count == 0)
            return new CurrencyRatesSaveResult(0);

        var valuesBuilder = new StringBuilder();
        var parameters = new List<SqlParameter>();

        var lastUpdatedUtc = _timeProvider.GetUtcNow().UtcDateTime;

        for (int i = 0; i < currencyRates.Count; i++)
        {
            var codeParam = $"@CurrencyCode{i}";
            var rateParam = $"@Rate{i}";
            var dateParam = $"@Date{i}";
            var lastUpdatedParam = $"@LastUpdatedUtc{i}";

            valuesBuilder.AppendLine(
                i == currencyRates.Count - 1
                    ? $"({codeParam}, {rateParam}, {dateParam}, {lastUpdatedParam})"
                    : $"({codeParam}, {rateParam}, {dateParam}, {lastUpdatedParam}),"
            );

            var r = currencyRates[i];

            parameters.Add(new SqlParameter(codeParam, r.CurrencyCode));
            parameters.Add(new SqlParameter(rateParam, r.Rate));
            parameters.Add(new SqlParameter(dateParam, r.Date));
            parameters.Add(new SqlParameter(lastUpdatedParam, lastUpdatedUtc));
        }

        var sql = $@"
            MERGE INTO CurrencyRates AS Target
            USING (
                VALUES
                    {valuesBuilder}
            ) AS Source (CurrencyCode, Rate, Date, LastUpdatedUtc)
            ON Target.CurrencyCode = Source.CurrencyCode
               AND Target.Date = Source.Date

            WHEN MATCHED AND Target.Rate <> Source.Rate THEN
                UPDATE SET
                    Target.Rate = Source.Rate,
                    Target.LastUpdatedUtc = Source.LastUpdatedUtc

            WHEN NOT MATCHED THEN
                INSERT (CurrencyCode, Rate, Date, LastUpdatedUtc)
                VALUES (Source.CurrencyCode, Source.Rate, Source.Date, Source.LastUpdatedUtc);
        ";

        var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(
            sql,
            parameters.ToArray(),
            cancellationToken
        );

        return new CurrencyRatesSaveResult(affectedRows);
    }
}
