using Novibet.Assessment.Application.Features.CurrencyRates;
using Novibet.Assessment.Application.Features.Wallets.Exceptions;
using Novibet.Assessment.Domain.Entities;

namespace Novibet.Assessment.Application.Features.Wallets;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrencyRateRepository _currencyRateRepository;
    private readonly Dictionary<WalletAdjustmentStrategy, WalletAdjustmentAction> _strategies = new();
    private readonly TimeProvider _timeProvider;
    private delegate void WalletAdjustmentAction(Wallet wallet, decimal amount);


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



    public async Task<Wallet> CreateWalletAsync(string currency, decimal initialBalance = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidCurrencyException(nameof(currency));

        var wallet = new Wallet
        {
            Currency = currency,
            Balance = initialBalance
        };

        return await _walletRepository.AddAsync(wallet, cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(long walletId, string? currency = null, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId, cancellationToken)
            ?? throw new WalletNotFoundException(walletId);

        if (!string.IsNullOrWhiteSpace(currency) &&
            !string.Equals(wallet.Currency, currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new CurrencyMismatchException(wallet.Currency, currency);
        }

        return wallet.Balance;
    }

    public async Task<Wallet> AdjustBalanceAsync(AdjustBalanceRequest request, CancellationToken cancellationToken = default)
    {
        if (_strategies is null || _strategies.Count == 0)
            throw new UnsupportedWalletStrategyException(nameof(_strategies));

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new InvalidCurrencyException(nameof(request.Currency));


        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.WalletId);

        var dateUtc = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var currencies = new[] { request.Currency, wallet.Currency };
        var rates = await _currencyRateRepository.GetCurrencyRates(currencies, dateUtc, cancellationToken);

        if (!rates.TryGetValue(request.Currency, out var requestCurrencyRate) || requestCurrencyRate <= 0m)
            throw new ResourceNotFoundException($"Currency rate for {request.Currency} on {dateUtc:yyyy-MM-dd}");

        if (!rates.TryGetValue(wallet.Currency, out var walletCurrencyRate) || walletCurrencyRate <= 0m)
            throw new ResourceNotFoundException($"Currency rate for {wallet.Currency} on {dateUtc:yyyy-MM-dd}");

        var amountInRequestCurrency = request.Amount / requestCurrencyRate;
        var amoutInWalletCurrency = Math.Round(amountInRequestCurrency * walletCurrencyRate, 2, MidpointRounding.ToZero);

        if (!_strategies.TryGetValue(request.Strategy, out var walletAdjustmentAction))
            throw new UnsupportedWalletStrategyException(request.Strategy.ToString());

        walletAdjustmentAction(wallet, amoutInWalletCurrency);

        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        return wallet;
    }


    private void AddFunds(Wallet wallet, decimal amount)
    {
        wallet.Balance += amount;
    }

    private void SubtractFunds(Wallet wallet, decimal amount)
    {
        if (wallet.Balance < amount)
            throw new InsufficientFundsException(wallet.Id, amount, wallet.Balance);

        wallet.Balance -= amount;
    }

    private void ForceSubtractFunds(Wallet wallet, decimal amount)
    {
        wallet.Balance -= amount;
    }

}
