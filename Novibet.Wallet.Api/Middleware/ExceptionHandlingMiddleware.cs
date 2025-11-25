using Novibet.Wallet.Api.Exceptions;
using Novibet.Wallet.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace Novibet.Wallet.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RateLimitException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, $"{ex.Message} Retry after: {ex.RetryAfter.TotalSeconds} seconds");

        }
        catch (BaseWalletException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            error = message,
            status = (int)statusCode
        });

        await context.Response.WriteAsync(payload);
    }
}

