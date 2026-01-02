using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Util;
using Android.Graphics;
using Android.Runtime;
using Android;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using Neo.Services;
using Neo.Storage;
using Neo.Constants;
using StringBuilder = System.Text.StringBuilder;
using static Android.Gms.Vision.Detector;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Exception = Java.Lang.Exception;
using View = Android.Views.View;

namespace NeoSoftware
{
    [Activity(Label = "Neo", Theme = "@style/AppTheme", MainLauncher = true)]
    public partial class MainActivity : AppCompatActivity, ISurfaceHolderCallback, IProcessor
    {
        private Solver _solver;
        private StorageHandler _storage;
        private SurfaceView _cameraView;
        private TextView _tessOutput;
        private CameraSource _cameraSource;
        private TextRecognizer _textRecognizer;
        private TextView _output;
        private View _confirmDataInput;
        private AlertDialog _confirmationAlertDialog;
        private Switch _detectSwitch;
        private bool _detect = false;
        private bool _isLoadMain = false;
        private bool _isEquations = false;

        private const int RequestCameraPermissionID = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.activity_main);
            _isLoadMain = true;

            BuildUi();
            _storage = new StorageHandler(ConnectionStringConstant.NpgsqlConnectionString);
            _solver = new Solver(_storage);
        }

        /// <summary>
        /// renders all ui of app
        /// </summary>
        private void BuildUi()
        {
            if (_isLoadMain)
            {
                ConfigureRecognizer();
                ConfigureConfirmationAlertDialog();
                ConfigureDetectSwitch();
            }
            else
            {
                InitAllElements();
                ConfigureRenderingInputFields();
            }
        }

        private void ConfigureDetectSwitch()
        {
            _detectSwitch = FindViewById<Switch>(Resource.Id.detect_switch);
            _detectSwitch.CheckedChange += delegate { _detect = !_detect; };
            _detectSwitch.Checked = _detect;
        }

        private void ConfigureRecognizer()
        {
            _cameraView = FindViewById<SurfaceView>(Resource.Id.surface_view);
            _tessOutput = FindViewById<TextView>(Resource.Id.tess_output);
            _output = FindViewById<TextView>(Resource.Id.output);
            _textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();

            if (!_textRecognizer.IsOperational)
            {
                Log.Error("Main Activity", "Detector dependencies are not yet available");
                throw new Exception($"{nameof(_textRecognizer.IsOperational)} is {_textRecognizer.IsOperational}");
            }

            try
            {
                _cameraSource = new CameraSource.Builder(ApplicationContext, _textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1555, 1080).SetRequestedFps(25f).SetAutoFocusEnabled(true).Build();
                _cameraView.Holder.AddCallback(this);
                _textRecognizer.SetProcessor(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void ConfigureConfirmationAlertDialog()
        {
            _confirmDataInput = LayoutInflater.Inflate(Resource.Layout.confirmation, null);
            _confirmationAlertDialog = new AlertDialog.Builder(this).Create();
            _confirmationAlertDialog.SetTitle("Confirm input");
            _confirmationAlertDialog.SetCancelable(true);
            _confirmationAlertDialog.SetView(_confirmDataInput);
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
            if (ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) !=
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

        public void ReceiveDetections(Detections detections)
        {
            var items = detections.DetectedItems;
            if (items.Size() == 0)
                return;
            if (!_detect)
                return;
            _tessOutput.Post(() =>
            {
                var strBuilder = new StringBuilder();
                for (var i = 0; i < items.Size(); ++i)
                {
                    strBuilder.Append(((TextBlock)items.ValueAt(i))?.Value);
                    strBuilder.Append("\n");
                }

                _tessOutput.Text = strBuilder.ToString();
                Thread.Sleep(700);
            });
        }

        public void Release()
        {
        }
    }
}