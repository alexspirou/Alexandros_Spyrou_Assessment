using Novibet.Assessment.Application.Interfaces;
using Novibet.Assessment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure();

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
