using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.Runtime;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Google.Android.Material.Tabs.AppCompat.App;
using Java.Interop;
using Java.Lang;
using Neo.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using StringBuilder = System.Text.StringBuilder;

namespace Neo.Droid
{
    [Activity(Label = "Neo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : FormsAppCompatActivity, ISurfaceHolderCallback, Detector.IProcessor
    {
        private SurfaceView _cameraView;
        private TextView _txtView;
        private CameraSource _cameraSource;
        private TextRecognizer _textRecognizer;
        private TextView _output;


        private
            const int RequestCameraPermissionID = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource  
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) !=
                Android.Content.PM.Permission.Granted)
            {
                //Request permission  
                ActivityCompat.RequestPermissions(this, new[]
                {
                    Manifest.Permission.Camera
                }, RequestCameraPermissionID);
                return;
            }

            _cameraSource.Start(_cameraView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            _cameraSource.Stop();
        }

        public void ReceiveDetections(Detector.Detections detections)
        {
            var items = detections.DetectedItems;
            if (items.Size() == 0)
                return;
            _txtView.Post(() =>
            {
                var strBuilder = new StringBuilder();
                for (var i = 0; i < items.Size(); ++i)
                {
                    strBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                    strBuilder.Append("\n");
                }

                _txtView.Text = strBuilder.ToString();
                Thread.Sleep(500);
            });
        }

        public void Release()
        {
        }

        [Export("Solve")]
        public async void Solve(Android.Views.View view)
        {
            try
            {
                _output.Text = (await new Solver().SolveAsync(_txtView.Text)).ToString();
            }
            catch (Exception ex)
            {
                // await DisplayAlert("Oh... something went wrong :(",
                //     $"inner: {ex.InnerException?.Message}\nmessage: {ex.Message}",
                //     "OK");
                throw new Exception(ex);
            }
        }
    }
}