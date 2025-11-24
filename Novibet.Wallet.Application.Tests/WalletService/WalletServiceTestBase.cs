using Microsoft.Extensions.Time.Testing;
using Moq;
using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets;
using System;

namespace Novibet.Wallet.Application.Tests.Services;

public abstract class WalletServiceTestBase
{
    protected readonly Mock<IWalletRepository> WalletRepoMock = new();
    protected readonly Mock<ICurrencyRateRepository> RatesRepoMock = new();
    protected readonly FakeTimeProvider TimeProvider = new(new DateTimeOffset(2025, 11, 22, 0, 0, 0, TimeSpan.Zero));

    protected WalletService CreateSut() =>
        new WalletService(WalletRepoMock.Object, RatesRepoMock.Object, TimeProvider);
}
