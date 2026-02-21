using Microsoft.Extensions.Logging;
using Neo.Application.Solver.Equation;
using Neo.CompositionRoot;
using NeoAndroidApp.Converters;
using NeoAndroidApp.ViewModels;
using NeoAndroidApp.Views;

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

        // Register Neo equation solver
        builder.Services.AddNeoEquationSolver(options =>
        {
            options.EnableCaching = true;
            options.CacheTtl = TimeSpan.FromMinutes(30);
            options.DefaultAlgorithm = SolvingAlgorithm.LU;
            options.ValidationTolerance = 1e-10;
        });

        // Register ViewModels and Views
        builder.Services.AddTransient<MatrixBuilderViewModel>();
        builder.Services.AddTransient<MatrixBuilderPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
