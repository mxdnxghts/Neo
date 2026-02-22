using NeoAndroidApp.Views;
using NeoAndroidApp.ViewModels;

namespace NeoAndroidApp;

/// <summary>
/// Main page of the Neo mobile application.
/// Provides access to the Matrix Builder feature.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the MainPage.
    /// </summary>
    public MainPage()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Handles the file import button click.
    /// </summary>
    private async void ReadFileOnImport(object? sender, EventArgs e)
	{
        try
        {
            var fileData = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an equation file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "txt", "eq" } },
                    { DevicePlatform.Android, new[] { "text/*", ".eq" } },
                    { DevicePlatform.WinUI, new[] { ".txt", ".eq" } }
                })
            });

            if (fileData != null)
            {
                // TODO: Implement file reading and equation parsing
                await DisplayAlertAsync("Info", "File selection feature coming soon", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to pick file: {ex.Message}", "OK");
        }
	}

    /// <summary>
    /// Handles the Matrix Builder button click to navigate to the builder page.
    /// </summary>
    private async void OnMatrixBuilderClicked(object? sender, EventArgs e)
    {
        try
        {
            var viewModel = Application.Current!.Handler!.MauiContext!.Services
                .GetRequiredService<MatrixBuilderViewModel>();
            var page = new MatrixBuilderPage(viewModel);
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to open Matrix Builder: {ex.Message}", "OK");
        }
    }
}