using Novibet.Assessment.Application;
using Novibet.Assessment.Application.Features.CurrencyRates;
using Novibet.Assessment.Infrastructure;
using Novibet.Assessment.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();

var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");

if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
    throw new InvalidOperationException("Database connection string is missing. Add it in User Secrets.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(new InfrastructureSettings(sqlServerConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/ecb/test", async (ICurrencyRateUpdater updater, CancellationToken ct) =>
{
    var result = await updater.UpdateRatesAsync(ct);
    return Results.Ok($"Rows affected {result.AffectedRows}");
})
.WithName("TestEcbRates")
.WithOpenApi();

app.Run();
