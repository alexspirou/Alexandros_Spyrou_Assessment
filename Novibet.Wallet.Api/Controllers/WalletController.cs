using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Application.Features.Wallets.Responses;
using Novibet.Wallet.Application.Features.Wallets.Services;

namespace Novibet.Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(
        IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWalletRequest request,
        [FromServices] IValidator<CreateWalletRequest> createValidator, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ToDictionary(validation)));
        }

        var walletId = await _walletService.CreateWalletAsync(request.Currency, request.InitialBalance, cancellationToken);
        return CreatedAtAction(nameof(Create), new { walletId }, new { walletId });
    }


    [HttpGet("{walletId:long}")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long walletId,
        [FromQuery] GetBalanceQuery query,
        [FromServices] IValidator<GetBalanceQuery> getBalanceValidator,
        CancellationToken cancellationToken)
    {
        var validation = await getBalanceValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ToDictionary(validation)));
        }

        var walletResponse = await _walletService.GetBalanceAsync(walletId, query.Currency, cancellationToken);
        return Ok(walletResponse);
    }

    [HttpPost("{walletId:long}/adjustbalance")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustBalance(long walletId, [FromQuery] AdjustBalanceQuery query,
        [FromServices] IValidator<AdjustBalanceQuery> adjustValidator,
        CancellationToken cancellationToken)
    {
        var validation = await adjustValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ToDictionary(validation)));
        }

        var walletResponse = await _walletService.AdjustBalanceAsync(
            new AdjustBalanceRequest(walletId, query.Amount, query.Currency, query.Strategy),
            cancellationToken);

        return Ok(walletResponse);
    }

    private static Dictionary<string, string[]> ToDictionary(ValidationResult validationResult) =>
        new()
        {
            ["Errors"] = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
        };
}
