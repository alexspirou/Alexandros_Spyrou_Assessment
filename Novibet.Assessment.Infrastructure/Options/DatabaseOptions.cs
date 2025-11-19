namespace Novibet.Assessment.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string OptionPath = nameof(DatabaseOptions);

    public string ConnectionString { get; set; } = string.Empty;
}
