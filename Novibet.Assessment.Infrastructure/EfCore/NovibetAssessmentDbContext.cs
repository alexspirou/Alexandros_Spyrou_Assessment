using Microsoft.EntityFrameworkCore;
using Novibet.Assessment.Domain.Entities;
using Novibet.Assessment.Infrastructure.Persistence.Configurations;

namespace Novibet.Assessment.Infrastructure.Persistence;

public class NovibetAssessmentDbContext(DbContextOptions<NovibetAssessmentDbContext> options)
    : DbContext(options)
{
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CurrencyRateConfiguration());
    }
}
