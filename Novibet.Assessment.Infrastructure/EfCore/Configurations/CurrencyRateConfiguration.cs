using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novibet.Assessment.Domain.Entities;

namespace Novibet.Assessment.Infrastructure.Persistence.Configurations;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("CurrencyRates");

        builder.HasKey(rate => rate.Id);

        builder.Property(rate => rate.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        builder.HasIndex(rate => new { rate.CurrencyCode, rate.Date }).IsUnique();

        builder.Property(rate => rate.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(rate => rate.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(rate => rate.Date)
            .HasConversion(
                date => date.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value))
            .IsRequired();

        builder.Property(rate => rate.LastUpdatedUtc)
            .IsRequired();
    }
}
