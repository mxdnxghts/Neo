using Microsoft.Extensions.DependencyInjection;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;
using Neo.Infrastructure.Telemetry;
using System;

namespace Neo.CompositionRoot;

/// <summary>
/// Extension methods for registering Neo services with dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all Neo equation solver services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure solving options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNeoEquationSolver(
        this IServiceCollection services,
        Action<SolvingOptions>? configureOptions = null)
    {
        // Options
        services.AddSingleton(sp =>
        {
            var options = new SolvingOptions
            {
                EnableCaching = true,
                CacheTtl = TimeSpan.FromMinutes(30),
                DefaultAlgorithm = SolvingAlgorithm.LU,
                ValidationTolerance = 1e-10,
                MaxDegreeOfParallelism = -1
            };
            configureOptions?.Invoke(options);
            return options;
        });

        // Domain Layer - no dependencies, pure domain objects
        // Domain objects are created by factories/services, not registered in DI

        // Infrastructure Layer - concrete implementations
        services.AddMemoryCache();
        services.AddSingleton<IEquationParser, EquationParser>();
        services.AddSingleton<IMatrixConverter, MatrixConverter>();
        services.AddSingleton<IMatrixSolver, MatrixSolver>();
        services.AddSingleton<ISolutionValidator, SolutionValidator>();
        services.AddSingleton<IEquationCache, MemoryEquationCache>();
        services.AddSingleton<PerformanceMonitor>();

        // Application Layer - orchestrators
        services.AddSingleton<IEquationSolver, EquationSolver>();

        return services;
    }

    /// <summary>
    /// Adds Neo equation solver services with memory caching disabled.
    /// Useful for scenarios where caching is not desired.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure solving options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNeoEquationSolverWithoutCache(
        this IServiceCollection services,
        Action<SolvingOptions>? configureOptions = null)
    {
        // Options - disable caching by default
        services.AddSingleton(sp =>
        {
            var options = new SolvingOptions
            {
                EnableCaching = false
            };
            configureOptions?.Invoke(options);
            return options;
        });

        // Infrastructure Layer
        services.AddSingleton<IEquationParser, EquationParser>();
        services.AddSingleton<IMatrixConverter, MatrixConverter>();
        services.AddSingleton<IMatrixSolver, MatrixSolver>();
        services.AddSingleton<ISolutionValidator, SolutionValidator>();
        services.AddSingleton<IEquationCache, NullEquationCache>(); // No-op cache
        services.AddSingleton<PerformanceMonitor>();

        // Application Layer
        services.AddSingleton<IEquationSolver, EquationSolver>();

        return services;
    }

    /// <summary>
    /// Adds Neo equation solver services with a custom cache implementation.
    /// </summary>
    /// <typeparam name="TCache">The cache implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure solving options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNeoEquationSolverWithCustomCache<TCache>(
        this IServiceCollection services,
        Action<SolvingOptions>? configureOptions = null)
        where TCache : class, IEquationCache
    {
        // Options
        services.AddSingleton(sp =>
        {
            var options = new SolvingOptions();
            configureOptions?.Invoke(options);
            return options;
        });

        // Infrastructure Layer
        services.AddMemoryCache();
        services.AddSingleton<IEquationParser, EquationParser>();
        services.AddSingleton<IMatrixConverter, MatrixConverter>();
        services.AddSingleton<IMatrixSolver, MatrixSolver>();
        services.AddSingleton<ISolutionValidator, SolutionValidator>();
        services.AddSingleton<IEquationCache, TCache>();
        services.AddSingleton<PerformanceMonitor>();

        // Application Layer
        services.AddSingleton<IEquationSolver, EquationSolver>();

        return services;
    }

    /// <summary>
    /// Adds Neo equation solver services with full telemetry support.
    /// Uses the decorator pattern to wrap the core solver with telemetry concerns.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure solving options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNeoEquationSolverWithTelemetry(
        this IServiceCollection services,
        Action<SolvingOptions>? configureOptions = null)
    {
        // Register telemetry and performance monitoring
        services.AddSingleton<NeoTelemetryService>();
        services.AddSingleton<PerformanceMonitor>();
        
        // Register core solver components
        services.AddSingleton<IEquationParser, EquationParser>();
        services.AddSingleton<IMatrixConverter, MatrixConverter>();
        services.AddSingleton<IMatrixSolver, MatrixSolver>();
        services.AddSingleton<ISolutionValidator, SolutionValidator>();
        
        // Register options
        services.AddSingleton(sp =>
        {
            var options = new SolvingOptions
            {
                EnableCaching = true,
                CacheTtl = TimeSpan.FromMinutes(30),
                DefaultAlgorithm = SolvingAlgorithm.LU,
                ValidationTolerance = 1e-10,
                MaxDegreeOfParallelism = -1
            };
            configureOptions?.Invoke(options);
            return options;
        });
        
        // Register cache
        services.AddMemoryCache();
        services.AddSingleton<IEquationCache, MemoryEquationCache>();
        
        // Register core solver
        services.AddSingleton<IEquationSolver, EquationSolver>();
        services.Decorate<IEquationSolver, TelemetryEquationSolverDecorator>();
        // services.AddSingleton<IEquationSolver>(sp =>
        // {
        //     var parser = sp.GetRequiredService<IEquationParser>();
        //     var converter = sp.GetRequiredService<IMatrixConverter>();
        //     var matrixSolver = sp.GetRequiredService<IMatrixSolver>();
        //     var validator = sp.GetRequiredService<ISolutionValidator>();
        //     var options = sp.GetRequiredService<SolvingOptions>();
            
        //     // Create core solver WITHOUT cache - cache is handled by decorator
        //     var coreSolver = new EquationSolver(parser, converter, matrixSolver, validator, new NullEquationCache(), options);
            
        //     // Wrap with telemetry decorator (which also handles caching)
        //     var telemetry = sp.GetRequiredService<NeoTelemetryService>();
        //     var perfMonitor = sp.GetRequiredService<PerformanceMonitor>();
        //     var cache = sp.GetRequiredService<IEquationCache>();
        //     return new TelemetryEquationSolverDecorator(coreSolver, telemetry, perfMonitor, cache, options);
        // });

        return services;
    }
}