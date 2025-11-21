namespace Novibet.Wallet.Infrastructure.Options;

public sealed class BackgroundWorkerSettings
{
    public const string OptionsPath = nameof(BackgroundWorkerSettings);

    public bool Enabled { get; init; } = false;
}
