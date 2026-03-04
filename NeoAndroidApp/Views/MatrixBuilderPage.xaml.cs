using NeoAndroidApp.ViewModels;

namespace NeoAndroidApp.Views;

/// <summary>
/// Matrix Builder page for visually constructing and solving linear equation systems.
/// </summary>
public partial class MatrixBuilderPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the MatrixBuilderPage.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    public MatrixBuilderPage(MatrixBuilderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
