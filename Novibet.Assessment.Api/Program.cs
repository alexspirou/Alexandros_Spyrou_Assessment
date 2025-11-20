using Hangfire;
using Novibet.Assessment.Api;
using Novibet.Assessment.Api.Middleware;
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
builder.Services.Configure<HangfireOptions>(
    builder.Configuration.GetSection(HangfireOptions.OptionsPath));

var backGroundWorkerSettings = builder.Configuration
    .GetSection(BackgroundWorkerSettings.OptionsPath)
    .Get<BackgroundWorkerSettings>()
    ?? throw new InvalidOperationException("BackgroundWorkerSettings are missing. Add them in configuration.");

AddInfrastucture(builder, backGroundWorkerSettings.Enabled);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandling();

AddBackgroundJobs(app.Services, builder.Configuration, backGroundWorkerSettings.Enabled);

if (backGroundWorkerSettings.Enabled)
{
    app.UseHangfireDashboard();
}

app.MapGet("/ecb/test", async (ICurrencyRateUpdater updater, CancellationToken ct) =>
{
    var result = await updater.UpdateRatesAsync(ct);
    return Results.Ok($"Rows affected {result.AffectedRows}");
})
.WithName("TestEcbRates")
.WithOpenApi();

app.MapWalletEndpoints();


app.Run();





static void AddBackgroundJobs(IServiceProvider services, IConfiguration configuration, bool backgroundWorkerEnabled)
{
    using var scope = services.CreateScope();



    bool currencyRatesEnabled = configuration.GetValue<bool>("CurrencyRatesJob:Enabled");
    string? cron = configuration.GetValue<string>("CurrencyRatesJob:Cron");

    if (!currencyRatesEnabled || !backgroundWorkerEnabled)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("BackgroundJobs");

        logger.LogInformation("CurrencyRatesJob is disabled. No recurring job will be registered.");
        return;
    }

    var jobRegistration = scope.ServiceProvider.GetRequiredService<IJobRegistration>();
    jobRegistration.RegisterCurrencyRatesJobs(cron ?? "* * * * * *");
}
static void AddInfrastucture(WebApplicationBuilder builder, bool backGroundServiceEnabled)
{
    var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");

    if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
        throw new InvalidOperationException("Database connection string is missing. Add it in User Secrets.");

    var hangfireOptions = builder.Configuration
        .GetSection(HangfireOptions.OptionsPath)
        .Get<HangfireOptions>() ?? throw new InvalidOperationException("Hangfire options are missing. Ensure HangfireOptions exists in configuration.");

    builder.Services.AddInfrastructure(new InfrastructureSettings(sqlServerConnectionString, backGroundServiceEnabled, hangfireOptions));
}
