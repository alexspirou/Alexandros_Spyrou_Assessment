using Microsoft.Extensions.DependencyInjection;
using Novibet.Assessment.EcbGateway;

namespace Novibet.Assessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddEcbGateway();
        return services;
    }
}
