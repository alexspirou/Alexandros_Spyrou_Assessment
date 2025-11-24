using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Novibet.Wallet.Application.Features.Wallets;
using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Application.Features.Wallets.Responses;

namespace Novibet.Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IValidator<CreateWalletRequest> _createValidator;
    private readonly IValidator<AdjustBalanceQuery> _adjustValidator;

    public WalletController(
        IWalletService walletService,
        IValidator<CreateWalletRequest> createValidator,
        IValidator<AdjustBalanceQuery> adjustValidator)
    {
        _walletService = walletService;
        _createValidator = createValidator;
        _adjustValidator = adjustValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWalletRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ToDictionary(validation)));
        }

        var walletId = await _walletService.CreateWalletAsync(request.Currency, request.InitialBalance, ct);
        return CreatedAtAction(nameof(Get), new { walletId }, new { walletId });
    }


    [HttpGet("{walletId:long}")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long walletId, [FromQuery] string? currency, CancellationToken ct)
    {
        var balance = await _walletService.GetBalanceAsync(walletId, currency, ct);
        return Ok(new WalletBalanceResponse(walletId, balance, currency ?? "EUR"));
    }

    [HttpPost("{walletId:long}/adjustbalance")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustBalance(long walletId, [FromQuery] AdjustBalanceQuery query, CancellationToken cancellationToken)
    {
        var validation = await _adjustValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ToDictionary(validation)));
        }

        var wallet = await _walletService.AdjustBalanceAsync(
            new AdjustBalanceRequest(walletId, query.Amount, query.Currency, query.Strategy),
            cancellationToken);

        return Ok(new WalletResponse(
            Id: wallet.Id,
            Balance: wallet.Balance,
            Currency: wallet.Currency));
    }

    private static Dictionary<string, string[]> ToDictionary(ValidationResult validationResult) =>
        new()
        {
            ["Errors"] = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
        };
}
