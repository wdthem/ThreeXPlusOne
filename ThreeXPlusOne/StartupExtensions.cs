using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne;

public static class StartupExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Settings>(configuration);

        services.AddScoped<IProcess, Process>();
        services.AddScoped<IDirectedGraph, TwoDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraph, ThreeDimensionalDirectedGraph>();

        return services;
    }
}