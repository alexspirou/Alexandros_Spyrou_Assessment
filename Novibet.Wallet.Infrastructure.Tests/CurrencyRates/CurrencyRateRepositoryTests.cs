using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Novibet.Wallet.Application.Features.CurrencyRates.Responses;
using Novibet.Wallet.Infrastructure.EfCore.Repositories;
using Novibet.Wallet.Infrastructure.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novibet.Wallet.Infrastructure.Tests.CurrencyRates;

public class CurrencyRateRepositoryTests : IClassFixture<NovibetWalletApiFactory>
{
    private readonly NovibetWalletApiFactory _fixture;
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 11, 24, 0, 0, 0, TimeSpan.Zero));

    public CurrencyRateRepositoryTests(NovibetWalletApiFactory fixture)
    {
        _fixture = fixture;
    }

    private async Task CleanDb()
    {
        await using var dbContextWithScope = _fixture.CreateDbWithScope();
        var dbContext = dbContextWithScope.DbContext;
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [wl].[CurrencyRates]");
    }

    [Fact]
    public async Task SaveCurrencyRatesAsync_InsertsNewRates()
    {
        try
        {
            var currencyRates = new[]
            {
                new CurrencyRatesResponse("USD", 1m, new DateTime(2025, 11, 24)),
                new CurrencyRatesResponse("GBP", 2m, new DateTime(2025, 11, 24)),
            };

            await using var writeScope = _fixture.CreateDbWithScope();
            var repository = new CurrencyRateRepository(writeScope.DbContext, _timeProvider);

            var result = await repository.SaveCurrencyRatesAsync(currencyRates);

            result.AffectedRows.Should().Be(currencyRates.Length);
            result.HasChanges.Should().BeTrue();

            await using var readScope = _fixture.CreateDbWithScope();
            var storedRates = await readScope.DbContext.CurrencyRates
                .AsNoTracking()
                .OrderBy(x => x.CurrencyCode)
                .ToListAsync();

            storedRates.Should().HaveCount(currencyRates.Length);
            storedRates.Select(rate => rate.CurrencyCode)
                .Should()
                .BeEquivalentTo(new[] { "GBP", "USD" });

            storedRates.First(rate => rate.CurrencyCode == "USD").Rate.Should().Be(1m);
            storedRates.First(rate => rate.CurrencyCode == "GBP").Rate.Should().Be(2m);

            var expectedLastUpdated = _timeProvider.GetUtcNow().UtcDateTime;
            foreach (var rate in storedRates)
            {
                rate.LastUpdatedUtc.Should().BeCloseTo(expectedLastUpdated, TimeSpan.FromSeconds(1));
            }
        }
        finally
        {
            await CleanDb();
        }
    }

    [Fact]
    public async Task SaveCurrencyRatesAsync_UpdatesExistingRateWhenValueChanges()
    {
        var date = new DateTime(2025, 11, 24);
        try
        {
            var initialRates = new[]
            {
                new CurrencyRatesResponse("USD", 1m, date),
                new CurrencyRatesResponse("GBP", 2m, date),
            };

            var initialTimestamp = _timeProvider.GetUtcNow().UtcDateTime;

            await using (var seedScope = _fixture.CreateDbWithScope())
            {
                var seedRepository = new CurrencyRateRepository(seedScope.DbContext, _timeProvider);
                await seedRepository.SaveCurrencyRatesAsync(initialRates);
            }

            _timeProvider.Advance(TimeSpan.FromHours(2));

            var updatedRates = new[]
            {
                new CurrencyRatesResponse("USD", 2m, date),
                new CurrencyRatesResponse("GBP", 2m, date),
            };

            await using var writeScope = _fixture.CreateDbWithScope();
            var repo = new CurrencyRateRepository(writeScope.DbContext, _timeProvider);

            var result = await repo.SaveCurrencyRatesAsync(updatedRates);

            result.AffectedRows.Should().Be(1);
            result.HasChanges.Should().BeTrue();

            await using var readScope = _fixture.CreateDbWithScope();
            var storedRates = await readScope.DbContext.CurrencyRates
                .AsNoTracking()
                .OrderBy(x => x.CurrencyCode)
                .ToListAsync();

            var usdRate = storedRates.First(rate => rate.CurrencyCode == "USD");
            usdRate.Rate.Should().Be(2m);
            usdRate.LastUpdatedUtc.Should().BeCloseTo(_timeProvider.GetUtcNow().UtcDateTime, TimeSpan.FromSeconds(1));

            var gbpRate = storedRates.First(rate => rate.CurrencyCode == "GBP");
            gbpRate.Rate.Should().Be(2m);
            gbpRate.LastUpdatedUtc.Should().BeCloseTo(initialTimestamp, TimeSpan.FromSeconds(1));
        }
        finally
        {
            await CleanDb();
        }
    }

    [Fact]
    public async Task SaveCurrencyRatesAsync_InsertsNewDateForExistingCurrency()
    {
        var baseDate = new DateTime(2025, 11, 24);
        try
        {
            var initialRates = new[]
            {
                new CurrencyRatesResponse("USD", 1m, baseDate),
            };

            await using (var seedScope = _fixture.CreateDbWithScope())
            {
                var seedRepository = new CurrencyRateRepository(seedScope.DbContext, _timeProvider);
                await seedRepository.SaveCurrencyRatesAsync(initialRates);
            }

            _timeProvider.Advance(TimeSpan.FromHours(1));

            var newDate = baseDate.AddDays(1);

            var newRates = new[]
            {
                new CurrencyRatesResponse("USD", 2m, newDate),
            };

            await using var writeScope = _fixture.CreateDbWithScope();
            var repositoryUnderTest = new CurrencyRateRepository(writeScope.DbContext, _timeProvider);

            var result = await repositoryUnderTest.SaveCurrencyRatesAsync(newRates);

            result.AffectedRows.Should().Be(1);
            result.HasChanges.Should().BeTrue();

            await using var readScope = _fixture.CreateDbWithScope();
            var storedRates = await readScope.DbContext.CurrencyRates
                .AsNoTracking()
                .Where(rate => rate.CurrencyCode == "USD")
                .ToListAsync();

            storedRates.Should().HaveCount(2);
            storedRates.Should().ContainSingle(rate => rate.Date == DateOnly.FromDateTime(baseDate));
            storedRates.Should().ContainSingle(rate => rate.Date == DateOnly.FromDateTime(newDate) && rate.Rate == 2m);
        }
        finally
        {
            await CleanDb();
        }
    }
}
