namespace Novibet.Assessment.Infrastructure.Options;

public sealed record InfrastructureSettings(string SqlServerConnectionString, HangfireOptions Hangfire, bool BackgroundServiceEnabled);