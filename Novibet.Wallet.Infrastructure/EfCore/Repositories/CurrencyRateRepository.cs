using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;
using Novibet.Wallet.Application.Features.CurrencyRates.Responses;
using Novibet.Wallet.Infrastructure.Persistence;
using System.Text;

namespace Novibet.Wallet.Infrastructure.EfCore.Repositories;

public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly NovibetWalletDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CurrencyRateRepository(NovibetWalletDbContext context, TimeProvider timeProvider)
    {
        _dbContext = context;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CurrencyRates
            .AsNoTracking()
            .Select(x => x.CurrencyCode)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<string, decimal>> GetCurrencyRates(IEnumerable<string> currencyCodes, DateOnly date, CancellationToken ct = default)
    {
        var codes = currencyCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.ToUpperInvariant())
            .Distinct()
            .ToArray();

        return await _dbContext.CurrencyRates
            .AsNoTracking()
            .Where(x => codes.Contains(x.CurrencyCode))
            .GroupBy(x => x.CurrencyCode)
            .Select(g => g.OrderByDescending(x => x.Date).First())
            .ToDictionaryAsync(x => x.CurrencyCode.ToUpperInvariant(), x => x.Rate, ct);
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
            MERGE INTO wl.CurrencyRates AS Target
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
        try
        {


            var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                parameters.ToArray(),
                cancellationToken
            );

            return new CurrencyRatesSaveResult(affectedRows);
        }
        catch (Exception ex)
        {
            throw;
        }

    }
}

