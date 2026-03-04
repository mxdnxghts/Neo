using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Telemetry;
using Neo.Application.Solver.Equation;
using Neo.CompositionRoot;
using Neo.Infrastructure.Telemetry;
using NeoAndroidApp.Converters;
using NeoAndroidApp.ViewModels;
using NeoAndroidApp.Views;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace NeoAndroidApp;

/// <summary>
/// Configures the MAUI application and dependency injection container.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates the MAUI application with all services registered.
    /// </summary>
    /// <returns>The configured MauiApp.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register converters
        builder.Services.AddSingleton<IsValidDoubleConverter>();
        builder.Services.AddSingleton<InverseBoolConverter>();

        // Register Neo equation solver with telemetry
        builder.Services.AddNeoEquationSolverWithTelemetry(options =>
        {
            options.EnableCaching = true;
            options.CacheTtl = TimeSpan.FromMinutes(30);
            options.DefaultAlgorithm = SolvingAlgorithm.LU;
            options.ValidationTolerance = 1e-10;
        });

        // Register ViewModels and Views
        builder.Services.AddTransient<MatrixBuilderViewModel>();
        builder.Services.AddTransient<MatrixBuilderPage>();

        // Configure OpenTelemetry for MAUI
        builder.ConfigureOpenTelemetry();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
    
    /// <summary>
    /// Configures OpenTelemetry for the MAUI application.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The builder for chaining.</returns>
    private static MauiAppBuilder ConfigureOpenTelemetry(this MauiAppBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddMeter("Neo.EquationSolver")
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource("NeoAndroidApp")
                    .AddSource("Neo.EquationSolver")
                    .AddHttpClientInstrumentation();
            });
        
        // Configure OTLP exporter for Aspire Dashboard (development only)
        // In production, configure OTEL_EXPORTER_OTLP_ENDPOINT environment variable
        builder.Services.ConfigureOpenTelemetryMeterProvider(options =>
        {
            options.AddOtlpExporter();
        });
        
        builder.Services.ConfigureOpenTelemetryTracerProvider(options =>
        {
            options.AddOtlpExporter();
        });
        
        return builder;
    }
}
