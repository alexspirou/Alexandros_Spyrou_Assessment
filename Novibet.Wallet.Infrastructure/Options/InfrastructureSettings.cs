namespace Novibet.Wallet.Infrastructure.Options;

public sealed record InfrastructureSettings(string SqlServerConnectionString, bool BackgroundServiceEnabled, HangfireOptions? Hangfire = null);
