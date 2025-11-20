using Hangfire;
using Novibet.Assessment.Application;
using Novibet.Assessment.Application.Features.CurrencyRates;
using Novibet.Assessment.Infrastructure;
using Novibet.Assessment.Infrastructure.BackgroundServices;
using Novibet.Assessment.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();



builder.Services.AddApplication();
AddInfrastucture(builder);

var app = builder.Build();

AddBackgroundJobs(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHangfireDashboard();

app.MapGet("/ecb/test", async (ICurrencyRateUpdater updater, CancellationToken ct) =>
{
    var result = await updater.UpdateRatesAsync(ct);
    return Results.Ok($"Rows affected {result.AffectedRows}");
})
.WithName("TestEcbRates")
.WithOpenApi();

app.Run();

static void AddBackgroundJobs(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var jobRegistration = scope.ServiceProvider.GetRequiredService<IJobRegistration>();

        jobRegistration.RegisterCurrencyRatesJobs();
    }
}

static void AddInfrastucture(WebApplicationBuilder builder)
{
    var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");

    if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
        throw new InvalidOperationException("Database connection string is missing. Add it in User Secrets.");

    var hangfireOptions = builder.Configuration
        .GetSection(HangfireOptions.OptionsPath)
        .Get<HangfireOptions>() ?? throw new InvalidOperationException("Hangfire options is not configured.");

    builder.Services.AddInfrastructure(new InfrastructureSettings(sqlServerConnectionString, hangfireOptions));
}