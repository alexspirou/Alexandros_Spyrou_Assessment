using FluentAssertions;
using Moq;
using Novibet.Wallet.Application.Exceptions;
using Novibet.Wallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Novibet.Wallet.Application.Tests.Services;

public class GetBalanceAsyncTests : WalletServiceTestBase
{

    [Fact]
    public async Task When_WalletNotFound_Throws()
    {
        // Arrange
        WalletRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WalletEntity?)null);

        var sut = CreateSut();
        var walletId = 42;
        var currency = "EUR";

        // Act
        Func<Task> act = () => sut.GetBalanceAsync(walletId, currency, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task When_CurrencyMatchesOrNull_ReturnsBalance()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 1, Balance = 100m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();

        // Act
        var resultNull = await sut.GetBalanceAsync(wallet.Id, null, CancellationToken.None);
        var resultSame = await sut.GetBalanceAsync(wallet.Id, "EUR", CancellationToken.None);

        // Assert
        resultNull.Balance.Should().Be(100m);
        resultSame.Balance.Should().Be(100m);
        resultSame.Currency.Should().Be("EUR");
        RatesRepoMock.Verify(r => r.GetCurrencyRates(It.IsAny<IEnumerable<string>>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_CurrencyDiffers_ConvertsBalance()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 2, Balance = 120m, Currency = "USD" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.2m },
            { "EUR", 1m }
        };
        RatesRepoMock.Setup(r => r.GetCurrencyRates(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        var sut = CreateSut();

        // Act
        var result = await sut.GetBalanceAsync(wallet.Id, "EUR", CancellationToken.None);

        // Assert
        result.Balance.Should().Be(100m);
    }
}
