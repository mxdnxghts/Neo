using Microsoft.Extensions.DependencyInjection;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;
using System;

namespace Neo.Infrastructure.Integration;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddEquationInfrastructure(
        this IServiceCollection services,
        Action<InfrastructureOptions>? configure = null)
    {
        var options = new InfrastructureOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IEquationParser, EquationParser>();
        services.AddSingleton<IMatrixConverter, MatrixConverter>();

        if (options.EnablePerformanceMonitoring)
        {
            services.AddSingleton<PerformanceMonitor>();
        }

        return services;
    }
}

public class InfrastructureOptions
{
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public bool UseParallelProcessing { get; set; } = true;
}
