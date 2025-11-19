using Novibet.Assessment.Application.Interfaces;
using Novibet.Assessment.Infrastructure;
using Novibet.Assessment.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();

var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");

if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
    throw new InvalidOperationException("Database connection string is missing. Add it in User Secrets.");

builder.Services.AddInfrastructure(new InfrastructureSettings(sqlServerConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/ecb/test", async (ICurrencyRatesService ratesService, CancellationToken ct) =>
    Results.Ok(await ratesService.GetLatestRates(ct)))
.WithName("TestEcbRates")
.WithOpenApi();

app.Run();
