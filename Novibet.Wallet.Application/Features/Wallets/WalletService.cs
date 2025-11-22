using Novibet.Wallet.Application.Features.CurrencyRates;
using Novibet.Wallet.Application.Features.Wallets.Exceptions;
using Novibet.Wallet.Domain.Entities;

namespace Novibet.Wallet.Application.Features.Wallets;

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

    public async Task<decimal> GetBalanceAsync(long walletId, string? toCurrency = null, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId, cancellationToken)
            ?? throw new WalletNotFoundException(walletId);

        if (string.IsNullOrEmpty(toCurrency) || wallet.Currency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return wallet.Balance;
        }

        return await ConvertAmountAsync(wallet.Balance, wallet.Currency, toCurrency, cancellationToken);
    }

    public async Task<WalletEntity> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.WalletId);

        var amoutInWalletCurrency = await ConvertAmountAsync(request.Amount, wallet.Currency, request.Currency, cancellationToken);

        if (!_strategies.TryGetValue(request.Strategy, out var walletAdjustmentAction))
            throw new UnsupportedWalletStrategyException(request.Strategy.ToString());

        walletAdjustmentAction(wallet, amoutInWalletCurrency);

        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        return wallet;
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

    private async Task<decimal> ConvertAmountAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken)
    {
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        if (fromCurrency == toCurrency)
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        var date = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var currencyCodes = new[] { fromCurrency, toCurrency };

        var rates = await _currencyRateRepository.GetCurrencyRates(currencyCodes, date, cancellationToken);

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
