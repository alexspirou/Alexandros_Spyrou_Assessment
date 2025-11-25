namespace Novibet.Wallet.Infrastructure.Options;

public class HangfireOptions
{
    public const string OptionsPath = "HangfireOptions";

    public bool DashboardIsEnabled { get; set; } = true;
    public string DashboardPath { get; set; } = "/hangfire";
    public JobStorageMode JobStorageMode { get; set; } = JobStorageMode.InMemory;
    public string ServerName { get; set; } = "hangfire-server";
}
