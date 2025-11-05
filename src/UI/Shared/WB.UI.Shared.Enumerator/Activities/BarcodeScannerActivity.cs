using Android.Content.PM;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services.Internals;
using Java.Util.Concurrent;
using Xamarin.Google.MLKit.Vision.Barcode.Common;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Activity(Label = "Barcode Scanner", 
        Theme = "@style/Theme.AppCompat.NoActionBar",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class BarcodeScannerActivity : AppCompatActivity
    {
        private PreviewView previewView;
        private ProcessCameraProvider cameraProvider;
        private IBarcodeScanner barcodeScanner;
        private bool isScanning = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.barcode_scanner);

            previewView = FindViewById<PreviewView>(Resource.Id.previewView);
            var cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            
            if (cancelButton != null)
            {
                cancelButton.Click += (_, _) =>
                {
                    QRBarcodeScanService.SetResult(null);
                    Finish();
                };
            }

            // Initialize ML Kit barcode scanner - scan all formats
            var options = new BarcodeScannerOptions.Builder()
                .SetBarcodeFormats(
                    Barcode.FormatQrCode,      // QR codes
                    Barcode.FormatEan13,       // EAN-13
                    Barcode.FormatEan8,        // EAN-8
                    Barcode.FormatUpcA,        // UPC-A
                    Barcode.FormatUpcE,        // UPC-E
                    Barcode.FormatCode128,     // Code-128
                    Barcode.FormatCode39,      // Code-39
                    Barcode.FormatCode93,      // Code-93
                    Barcode.FormatCodabar,     // Codabar
                    Barcode.FormatDataMatrix,  // Data Matrix
                    Barcode.FormatPdf417,      // PDF417
                    Barcode.FormatAztec        // Aztec
                )

                .Build();
            
            barcodeScanner = BarcodeScanning.GetClient(options);

            StartCamera();
        }

        private void StartCamera()
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(this);
            cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
            {
                try
                {
                    cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();
                    BindCameraUseCases();
                }
                catch (ExecutionException ex)
                {
                    var message = ex.Message ?? "Unknown error";
                    Toast.MakeText(this, "Error starting camera: " + message, ToastLength.Long)?.Show();
                }
                catch (Java.Lang.InterruptedException ex)
                {
                    var message = ex.Message ?? "Interrupted";
                    Toast.MakeText(this, "Error starting camera: " + message, ToastLength.Long)?.Show();
                }
            }), ContextCompat.GetMainExecutor(this));
        }

        private void BindCameraUseCases()
        {
            if (cameraProvider == null) return;

            // Unbind all use cases before rebinding
            cameraProvider.UnbindAll();

            // Select back camera
            var cameraSelector = new CameraSelector.Builder()
                .RequireLensFacing(CameraSelector.LensFacingBack)
                .Build();

            // Preview use case - let CameraX choose optimal settings
            var preview = new Preview.Builder()
                .SetTargetResolution(new Android.Util.Size(1280, 960)) // 4:3 aspect ratio
                .Build();
            var surfaceProvider = previewView.SurfaceProvider;
            if (surfaceProvider != null)
            {
                preview.SetSurfaceProvider(ContextCompat.GetMainExecutor(this), surfaceProvider);
            }

            // Image analysis use case for barcode scanning with higher resolution
            var imageAnalysis = new ImageAnalysis.Builder()
                .SetTargetResolution(new Android.Util.Size(1280, 960)) // 4:3 aspect ratio
                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                .Build();

            var analyzer = new BarcodeAnalyzer(this);
            imageAnalysis.SetAnalyzer(ContextCompat.GetMainExecutor(this), analyzer);

            // Bind use cases to camera
            cameraProvider.BindToLifecycle(this, cameraSelector, preview, imageAnalysis);
        }

        private void OnBarcodeDetected(string code, byte[] rawBytes)
        {
            if (!isScanning) return;
            
            isScanning = false;
            RunOnUiThread(() =>
            {
                QRBarcodeScanService.SetResult(new QRBarcodeScanResult
                {
                    Code = code,
                    RawBytes = rawBytes
                });
                Finish();
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            barcodeScanner?.Close();
            cameraProvider?.UnbindAll();
        }

        private class BarcodeAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
        {
            private readonly BarcodeScannerActivity activity;
            private bool isProcessing;

            public BarcodeAnalyzer(BarcodeScannerActivity activity)
            {
                this.activity = activity;
            }

            public void Analyze(IImageProxy imageProxy)
            {
                if (isProcessing || !activity.isScanning)
                {
                    imageProxy.Close();
                    return;
                }

                isProcessing = true;

                try
                {
                    var mediaImage = imageProxy.Image;
                    if (mediaImage != null)
                    {
                        var inputImage = InputImage.FromMediaImage(mediaImage, imageProxy.ImageInfo.RotationDegrees);
                        
                        activity.barcodeScanner.Process(inputImage)
                            .AddOnSuccessListener(new SuccessListener(activity, imageProxy, () => isProcessing = false))
                            .AddOnFailureListener(new FailureListener(imageProxy, () => isProcessing = false));
                    }
                    else
                    {
                        imageProxy.Close();
                        isProcessing = false;
                    }
                }
                catch
                {
                    imageProxy.Close();
                    isProcessing = false;
                }
            }
        }

        private class SuccessListener : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener
        {
            private readonly BarcodeScannerActivity activity;
            private readonly IImageProxy imageProxy;
            private readonly Action onComplete;

            public SuccessListener(BarcodeScannerActivity activity, IImageProxy imageProxy, Action onComplete)
            {
                this.activity = activity;
                this.imageProxy = imageProxy;
                this.onComplete = onComplete;
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                try
                {
                    // Try different ways to access the list
                    Java.Util.IList barcodes = null;
                    
                    // First try: direct cast to IList
                    barcodes = result as Java.Util.IList;
                    
                    // Second try: if result is ArrayList, cast to ArrayList first
                    if (barcodes == null && result is Java.Util.ArrayList)
                    {
                        var arrayList = (Java.Util.ArrayList)result;
                        barcodes = arrayList;
                    }
                    
                    // Third try: use JavaCast extension method
                    if (barcodes == null && result != null)
                    {
                        try
                        {
                            barcodes = result.JavaCast<Java.Util.IList>();
                        }
                        catch { /* ignore if JavaCast fails */ }
                    }
                    
                    if (barcodes != null && barcodes.Size() > 0)
                    {
                        var firstBarcode = barcodes.Get(0);
                        
                        // Try to get RawValue property using reflection since the type isn't resolved
                        var rawValueProp = firstBarcode?.GetType().GetProperty("RawValue");
                        var rawBytesMethod = firstBarcode?.GetType().GetMethod("GetRawBytes");
                        
                        if (rawValueProp != null && rawBytesMethod != null)
                        {
                            var code = rawValueProp.GetValue(firstBarcode) as string;
                            var rawBytes = rawBytesMethod.Invoke(firstBarcode, null) as byte[];
                            
                            if (!string.IsNullOrEmpty(code))
                            {
                                activity.OnBarcodeDetected(code, rawBytes);
                            }
                        }
                    }
                }
                finally
                {
                    imageProxy.Close();
                    onComplete?.Invoke();
                }
            }
        }

        private class FailureListener : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener
        {
            private readonly IImageProxy imageProxy;
            private readonly Action onComplete;

            public FailureListener(IImageProxy imageProxy, Action onComplete)
            {
                this.imageProxy = imageProxy;
                this.onComplete = onComplete;
            }

            public void OnFailure(Java.Lang.Exception e)
            {
                imageProxy.Close();
                onComplete?.Invoke();
            }
        }
    }
}
