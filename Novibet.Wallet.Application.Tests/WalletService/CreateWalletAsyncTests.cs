using FluentAssertions;
using Moq;
using Novibet.Wallet.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Novibet.Wallet.Application.Tests.Services;

public class CreateWalletAsyncTests : WalletServiceTestBase
{
    [Fact]
    public async Task When_Creating_AddsWallet()
    {
        // Arrange
        WalletEntity? captured = null;
        WalletRepoMock.Setup(r => r.AddAsync(It.IsAny<WalletEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WalletEntity w, CancellationToken _) =>
            {
                captured = w;
                w.Id = 10;
                return w;
            });

        var sut = CreateSut();

        // Act
        var walletId = await sut.CreateWalletAsync("EUR", cancellationToken: CancellationToken.None);

        // Assert
        walletId.Should().Be(10);
        captured.Should().NotBeNull();
        captured!.Currency.Should().Be("EUR");
        captured.Balance.Should().Be(0m);
        WalletRepoMock.Verify(r => r.AddAsync(It.IsAny<WalletEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
