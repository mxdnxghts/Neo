using Tesseract;
using IronOcr;

namespace NeoAndroidApp;

public partial class MainPage : ContentPage
{
    private TesseractEngine _engine;

    private readonly IronTesseract _ocrTesseract;

    public MainPage()
	{
		InitializeComponent();
        _ocrTesseract = new IronTesseract()
        {
            Language = OcrLanguage.Financial,
        };
	}

    public async Task InitializeAsync()
    {
        //var tessDataPath = Path.Combine(
        //    FileSystem.Current.AppDataDirectory,
        //    "TessData"
        //);

        //_engine = new TesseractEngine(
        //    datapath: tessDataPath,
        //    language: "eng+math",
        //    engineMode: EngineMode.LstmOnly
        //);
    }

    private async void ReadFileOnImport(object? sender, EventArgs e)
	{
		var picker = await MediaPicker.PickPhotosAsync();
		var fullPath = picker[0].FullPath;
        using var stream = await picker[0].OpenReadAsync();
        using var ocrInput = new OcrInput();
        ocrInput.LoadImage(stream);

        var ocrResult = await _ocrTesseract.ReadAsync(ocrInput);

        SemanticScreenReader.Announce(ocrResult.Text);
	}
}