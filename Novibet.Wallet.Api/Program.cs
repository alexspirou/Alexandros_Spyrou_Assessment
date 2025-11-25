using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Novibet.Wallet.Api;
using Novibet.Wallet.Api.Exceptions;
using Novibet.Wallet.Api.Middleware;
using Novibet.Wallet.Application;
using Novibet.Wallet.Application.Features.Wallets;
using Novibet.Wallet.Application.Features.Wallets.Requests;
using Novibet.Wallet.Infrastructure;
using Novibet.Wallet.Infrastructure.BackgroundServices;
using Novibet.Wallet.Infrastructure.Options;
using Novibet.Wallet.Infrastructure.Persistence;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();  // <- load .env

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<WalletAdjustmentStrategy>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames<WalletAdjustmentStrategy>()
            .Select(name => (IOpenApiAny)new OpenApiString(name))
            .ToList()
    });
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10)
            }));

    options.OnRejected = (context, token) =>
    {
        context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry);
        throw new RateLimitException("Too many requests.", retry);
    };
});


builder.Services.AddControllers();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddApplication();
builder.Services.Configure<HangfireOptions>(
    builder.Configuration.GetSection(HangfireOptions.OptionsPath));
builder.Services.AddValidatorsFromAssemblyContaining<CreateWalletRequest>();

ConfigureCache(builder);

var backGroundWorkerSettings = builder.Configuration
    .GetSection(BackgroundWorkerSettings.OptionsPath)
    .Get<BackgroundWorkerSettings>()
    ?? throw new InvalidOperationException("BackgroundWorkerSettings are missing. Add them in configuration.");

AddInfrastucture(builder, backGroundWorkerSettings.Enabled);

var app = builder.Build();

await ApplyMigrations(app);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

AddBackgroundJobs(app.Services, builder.Configuration, backGroundWorkerSettings.Enabled);

if (backGroundWorkerSettings.Enabled)
{
    app.UseHangfireDashboard();
}

app.UseRateLimiter();

app.MapControllers();

app.Run();




static void AddBackgroundJobs(IServiceProvider services, IConfiguration configuration, bool backgroundWorkerEnabled)
{
    using var scope = services.CreateScope();

    string? cron = configuration.GetValue<string>("CurrencyRatesJob:Cron");
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("BackgroundJobs");

    if (!backgroundWorkerEnabled)
    {
        logger.LogInformation("Background worker is disabled. No jobs will be registered.");
        return;
    }

    var jobRegistration = scope.ServiceProvider.GetRequiredService<IBackgroundJobConfigurator>();

    bool currencyRatesEnabled = configuration.GetValue<bool>("CurrencyRatesJob:Enabled");

    if (currencyRatesEnabled)
    {
        jobRegistration.RegisterCurrencyRatesJobs(cron ?? "* * * * * *");
        logger.LogInformation("Recurring CurrencyRatesJob registered.");
    }
    bool registerAvailableCurrenciesForCacheEnabled = configuration.GetValue<bool>("AvailableCurrenciesForCacheJob:Enabled");

    if (registerAvailableCurrenciesForCacheEnabled)
    {
        jobRegistration.RegisterAvailableCurrenciesForCache();
        logger.LogInformation("AvailableCurrenciesForCacheJob has started.");
    }
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

static void ConfigureCache(WebApplicationBuilder builder)
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");

    var fusionBuilder = builder.Services
        .AddFusionCache()
        .TryWithAutoSetup()
        .WithSerializer(new FusionCacheSystemTextJsonSerializer());

    if (!string.IsNullOrWhiteSpace(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
        });
    }
}

static async Task ApplyMigrations(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbMigration");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<NovibetWalletDbContext>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); 
            await db.Database.MigrateAsync(cancellationToken:cts.Token);
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Database migration timed out.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations.");
            throw;
        }
    }
}

public partial class Program { }
