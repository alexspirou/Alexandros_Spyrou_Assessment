using Novibet.Wallet.Application.Exceptions;
using Novibet.Wallet.Application.Features.CurrencyRates.Repositories;
using Novibet.Wallet.Application.Features.Wallets.Repositories;
using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Application.Features.Wallets.Responses;
using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Application.Features.Wallets.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrencyRateRepository _currencyRateRepository;
    private readonly Dictionary<WalletAdjustmentStrategy, WalletAdjustmentAction> _strategies = new();
    private readonly TimeProvider _timeProvider;
    private delegate void WalletAdjustmentAction(WalletEntity wallet, decimal amount);


    public WalletService(IWalletRepository walletRepository,
        ICurrencyRateRepository currencyRateRepository, TimeProvider timeProvider)
    {
        _walletRepository = walletRepository;
        _currencyRateRepository = currencyRateRepository;
        _timeProvider = timeProvider;

        _strategies.Add(WalletAdjustmentStrategy.AddFunds, AddFunds);
        _strategies.Add(WalletAdjustmentStrategy.SubtractFunds, SubtractFunds);
        _strategies.Add(WalletAdjustmentStrategy.ForceSubtractFunds, ForceSubtractFunds);
    }



    public async Task<long> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default)
    {
        var wallet = new WalletEntity
        {
            Currency = currency,
            Balance = initialBalance
        };

        wallet = await _walletRepository.AddAsync(wallet, cancellationToken);

        return wallet.Id;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(long walletId, string? toCurrency = null, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId, cancellationToken)
            ?? throw new WalletNotFoundException(walletId);

        decimal balance;
        if (string.IsNullOrEmpty(toCurrency) || wallet.Currency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            balance = wallet.Balance;
        }
        else
        {
            balance = await ConvertAmountAsync(wallet.Balance, wallet.Currency, toCurrency, cancellationToken);
        }

        return new WalletBalanceResponse(wallet.Id, balance, wallet.Currency);

    }

    public async Task<WalletBalanceResponse> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.WalletId);

        var amoutInWalletCurrency = await ConvertAmountAsync(request.Amount, wallet.Currency, request.Currency, cancellationToken);

        if (!_strategies.TryGetValue(request.Strategy, out var walletAdjustmentAction))
            throw new UnsupportedWalletStrategyException(request.Strategy.ToString());

        walletAdjustmentAction(wallet, amoutInWalletCurrency);

        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        return new WalletBalanceResponse(wallet.Id, wallet.Balance, wallet.Currency);
    }


    private void AddFunds(WalletEntity wallet, decimal amount)
    {
        wallet.Balance += amount;
    }

    private void SubtractFunds(WalletEntity wallet, decimal amount)
    {
        if (wallet.Balance < amount)
            throw new InsufficientFundsException(wallet.Id, amount, wallet.Balance);

        wallet.Balance -= amount;
    }

    private void ForceSubtractFunds(WalletEntity wallet, decimal amount)
    {
        wallet.Balance -= amount;
    }

    private async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        var date = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var currencyCodes = new[] { fromCurrency, toCurrency };

        var rates = await _currencyRateRepository
            .GetCurrencyRates(currencyCodes, date, cancellationToken);

        if (!rates.ContainsKey(CurrencyCodes.Eur))
            rates[CurrencyCodes.Eur] = 1m;

        if (!rates.TryGetValue(fromCurrency, out var fromRate))
            throw new CurrencyRateNotFoundException(fromCurrency, date);

        if (!rates.TryGetValue(toCurrency, out var toRate))
            throw new CurrencyRateNotFoundException(toCurrency, date);

        var amountInEur = amount / fromRate;
        var convertedAmount = amountInEur * toRate;

        return convertedAmount;
    }

}
