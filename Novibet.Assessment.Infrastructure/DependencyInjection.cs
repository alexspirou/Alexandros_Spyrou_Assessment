using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Novibet.Assessment.EcbGateway;
using Novibet.Assessment.Infrastructure.Options;
using Novibet.Assessment.Infrastructure.Persistence;

namespace Novibet.Assessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastructureSettings settings)
    {
        services.AddEcbGateway();

        if (string.IsNullOrWhiteSpace(settings.SqlServerConnectionString))
        {
            throw new InvalidOperationException("Database connection string is missing");
        }

        services.AddDbContext<NovibetAssessmentDbContext>(options =>
            options.UseSqlServer(settings.SqlServerConnectionString));

        return services;
    }
}
