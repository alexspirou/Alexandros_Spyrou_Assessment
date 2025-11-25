using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Novibet.Wallet.Infrastructure.Persistence;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace Novibet.Wallet.Infrastructure.Tests.Fixtures;

public class NovibetWalletApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DefaultPassword = "PassPass123!";

    private readonly MsSqlContainer _dbContainer;

    public NovibetWalletApiFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithPassword(DefaultPassword)
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<NovibetWalletDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<NovibetWalletDbContext>(a =>
            {
                a.UseSqlServer(_dbContainer.GetConnectionString());
            });
        });
    }


    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NovibetWalletDbContext>();
        await db.Database.MigrateAsync();
    }

    public DbContextWithScope CreateDbWithScope()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovibetWalletDbContext>();
        return new DbContextWithScope(scope, db);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync().AsTask();
        await base.DisposeAsync();
    }
}
