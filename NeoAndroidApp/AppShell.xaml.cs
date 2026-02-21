using NeoAndroidApp.Views;

namespace NeoAndroidApp;

/// <summary>
/// Application shell with routing configuration.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the AppShell.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    /// <summary>
    /// Registers all application routes.
    /// </summary>
    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(MatrixBuilderPage), typeof(MatrixBuilderPage));
    }
}
