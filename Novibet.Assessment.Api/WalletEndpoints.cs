using Microsoft.AspNetCore.Http;

namespace Novibet.Assessment.Api;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/wallets", () =>
        {
            // TODO: implement wallet creation
            return Results.StatusCode(StatusCodes.Status501NotImplemented);
        })
        .WithName("CreateWallet")
        .WithOpenApi();

        app.MapGet("/api/wallets/{walletId:long}", (long walletId, string? currency) =>
        {
            // TODO: implement wallet balance retrieval and optional currency conversion
            return Results.StatusCode(StatusCodes.Status501NotImplemented);
        })
        .WithName("GetWalletBalance")
        .WithOpenApi();

        app.MapPost("/api/wallets/{walletId:long}/adjustbalance", (long walletId, decimal amount, string currency, string strategy) =>
        {
            // TODO: implement balance adjustment based on strategy
            return Results.StatusCode(StatusCodes.Status501NotImplemented);
        })
        .WithName("AdjustWalletBalance")
        .WithOpenApi();
    }
}
