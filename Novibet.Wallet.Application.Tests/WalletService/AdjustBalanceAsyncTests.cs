using FluentAssertions;
using Moq;
using Novibet.Wallet.Application.Exceptions;
using Novibet.Wallet.Application.Features.Wallets;
using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Novibet.Wallet.Application.Tests.Services;

public class AdjustBalanceAsyncTests : WalletServiceTestBase
{
    [Fact]
    public async Task When_WalletMissing_Throws()
    {
        // Arrange
        WalletRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WalletEntity?)null);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(99, 10m, "EUR", WalletAdjustmentStrategy.AddFunds);

        // Act
        Func<Task> act = () => sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task When_StrategyUnsupported_Throws()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 1, Balance = 50m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 10m, "EUR", (WalletAdjustmentStrategy)999);

        // Act
        Func<Task> act = () => sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnsupportedWalletStrategyException>();
    }

    [Fact]
    public async Task When_AddFundsWithDifferentCurrency_UsesConversion()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 2, Balance = 100m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        WalletRepoMock.Setup(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 1m },
            { "USD", 1.2m }
        };

        var expectedDate = DateOnly.FromDateTime(TimeProvider.GetUtcNow().UtcDateTime);
        RatesRepoMock.Setup(r => r.GetCurrencyRates(
                It.Is<IEnumerable<string>>(codes => codes.Contains("EUR") && codes.Contains("USD")),
                expectedDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 50m, "USD", WalletAdjustmentStrategy.AddFunds);

        // Act
        var result = await sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        result.Balance.Should().Be(160m);
        RatesRepoMock.Verify(r => r.GetCurrencyRates(
            It.Is<IEnumerable<string>>(codes => codes.Contains("EUR") && codes.Contains("USD")),
            expectedDate,
            It.IsAny<CancellationToken>()), Times.Once);
        WalletRepoMock.Verify(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task When_SubtractFundsInsufficientBalance_Throws()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 3, Balance = 50m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 100m, "EUR", WalletAdjustmentStrategy.SubtractFunds);

        // Act
        Func<Task> act = () => sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientFundsException>();
        WalletRepoMock.Verify(r => r.UpdateAsync(It.IsAny<WalletEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_CurrencySame_ReturnsOriginalAmount_NoRatesLookup()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 4, Balance = 100m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        WalletRepoMock.Setup(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 10m, "EUR", WalletAdjustmentStrategy.AddFunds);

        // Act
        var result = await sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        result.Balance.Should().Be(110m);
        RatesRepoMock.Verify(r => r.GetCurrencyRates(It.IsAny<IEnumerable<string>>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_FromRateMissing_Throws()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 5, Balance = 100m, Currency = "USD" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var expectedDate = DateOnly.FromDateTime(TimeProvider.GetUtcNow().UtcDateTime);
        RatesRepoMock.Setup(r => r.GetCurrencyRates(
                It.IsAny<IEnumerable<string>>(),
                expectedDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { { "EUR", 1m } });

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 50m, "EUR", WalletAdjustmentStrategy.AddFunds);

        // Act
        Func<Task> act = () => sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CurrencyRateNotFoundException>();
        WalletRepoMock.Verify(r => r.UpdateAsync(It.IsAny<WalletEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_ToRateMissing_Throws()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 6, Balance = 100m, Currency = "EUR" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var expectedDate = DateOnly.FromDateTime(TimeProvider.GetUtcNow().UtcDateTime);
        RatesRepoMock.Setup(r => r.GetCurrencyRates(
                It.IsAny<IEnumerable<string>>(),
                expectedDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { { "EUR", 1m } });

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 50m, "USD", WalletAdjustmentStrategy.AddFunds);

        // Act
        Func<Task> act = () => sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CurrencyRateNotFoundException>();
        WalletRepoMock.Verify(r => r.UpdateAsync(It.IsAny<WalletEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_EurRateMissing_AddsDefaultAndConverts()
    {
        // Arrange
        var wallet = new WalletEntity { Id = 7, Balance = 100m, Currency = "USD" };
        WalletRepoMock.Setup(r => r.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        WalletRepoMock.Setup(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var expectedDate = DateOnly.FromDateTime(TimeProvider.GetUtcNow().UtcDateTime);
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.2m }
        };

        RatesRepoMock.Setup(r => r.GetCurrencyRates(
                It.IsAny<IEnumerable<string>>(),
                expectedDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        var sut = CreateSut();
        var request = new AdjustBalanceRequest(wallet.Id, 120m, "EUR", WalletAdjustmentStrategy.AddFunds);

        // Act
        var result = await sut.AdjustBalanceAsync(request, CancellationToken.None);

        // Assert
        result.Balance.Should().Be(200m);
        RatesRepoMock.Verify(r => r.GetCurrencyRates(It.IsAny<IEnumerable<string>>(), expectedDate, It.IsAny<CancellationToken>()), Times.Once);
        WalletRepoMock.Verify(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()), Times.Once);
    }
}
