using Android.Content.PM;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services.Internals;
using Java.Util.Concurrent;
using ZXing;
using AndroidX.Camera.Core.ResolutionSelector;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;

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
        private bool isScanning = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.barcode_scanner);

            previewView = FindViewById<PreviewView>(Resource.Id.previewView);

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
                    var message = ex.Message ?? UIResources.BarcodeScanner_Error_UnknownError;
                    Toast.MakeText(this, UIResources.BarcodeScanner_Error_StartingCamera + message, ToastLength.Long)?.Show();
                }
                catch (Java.Lang.InterruptedException ex)
                {
                    var message = ex.Message ?? UIResources.BarcodeScanner_Error_Interrupted;
                    Toast.MakeText(this, UIResources.BarcodeScanner_Error_StartingCamera + message, ToastLength.Long)?.Show();
                }
            }), ContextCompat.GetMainExecutor(this));
        }

        private IExecutorService analysisExecutor;
        private BarcodeAnalyzer analyzer;
        private Preview preview;
        private ImageAnalysis imageAnalysis;
        private CameraSelector cameraSelector;
        
        private void BindCameraUseCases()
        {
            if (cameraProvider == null) return;
            
            // Select back camera
            cameraSelector = new CameraSelector.Builder()
                .RequireLensFacing(CameraSelector.LensFacingBack)
                .Build();

            var resolutionSelector = new ResolutionSelector.Builder()
                .SetResolutionStrategy(new ResolutionStrategy(new Android.Util.Size(1280, 960), 
                    ResolutionStrategy.FallbackRuleClosestHigherThenLower))
                .Build();
            
            // Preview use case - keep decent quality for user
            preview?.Dispose();
            preview = new Preview.Builder()
                .SetResolutionSelector(resolutionSelector) // Lower resolution for speed
                .Build();
            var surfaceProvider = previewView.SurfaceProvider;
            if (surfaceProvider != null)
            {
                preview.SetSurfaceProvider(ContextCompat.GetMainExecutor(this), surfaceProvider);
            }

            // Image analysis use case - use lower resolution for faster processing
            imageAnalysis?.Dispose();
            imageAnalysis = new ImageAnalysis.Builder()
                .SetOutputImageFormat(ImageAnalysis.OutputImageFormatRgba8888)
                .SetResolutionSelector(resolutionSelector) // Lower resolution for speed
                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                .Build();

            analyzer?.Dispose();
            analyzer = new BarcodeAnalyzer(this); 
            
            // Shutdown old executor if exists
            analysisExecutor?.Shutdown();
            analysisExecutor?.Dispose();
            analysisExecutor = Java.Util.Concurrent.Executors.NewSingleThreadExecutor();
            imageAnalysis.SetAnalyzer(analysisExecutor, analyzer);

            // Bind use cases to camera
            cameraProvider.BindToLifecycle(this, cameraSelector, preview, imageAnalysis);
        }

        private void OnBarcodeDetected(string code, byte[] rawBytes)
        {
            if (!isScanning || IsFinishing || IsDestroyed) return;
            
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

        protected override void OnPause()
        {
            base.OnPause();
            // Stop scanning when going to background
            isScanning = false;
            cameraProvider?.UnbindAll();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Resume scanning when coming back
            if (cameraProvider != null && !IsFinishing && !IsDestroyed)
            {
                isScanning = true;
                BindCameraUseCases();
            }
        }
        
        public override void OnBackPressed()
        {
            QRBarcodeScanService.SetResult(null);
            base.OnBackPressed();
        }
        
        protected override void OnDestroy()
        {
            isScanning = false;

            imageAnalysis?.ClearAnalyzer();
            cameraProvider?.UnbindAll();

            analyzer?.Dispose();
            analyzer = null;

            imageAnalysis?.Dispose();
            imageAnalysis = null;

            analysisExecutor?.Shutdown();
            try
            {
                if (!analysisExecutor?.AwaitTermination(5, Java.Util.Concurrent.TimeUnit.Seconds) ?? false)
                {
                    analysisExecutor?.ShutdownNow();
                }
            }
            catch { }

            analysisExecutor?.Dispose();
            analysisExecutor = null;

            cameraSelector?.Dispose();
            cameraSelector = null;

            cameraProvider?.Dispose();
            cameraProvider = null;

            preview?.Dispose();
            preview = null;
            
            previewView?.Dispose();
            previewView = null;

            base.OnDestroy();
        }

        private class BarcodeAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
        {
            private readonly WeakReference<BarcodeScannerActivity> activityRef;
            private volatile bool isProcessing;
            private readonly BarcodeReaderGeneric reader;
            private long lastProcessTime;
            private const long MIN_FRAME_INTERVAL_MS = 100; // Process max 10 frames per second

            public BarcodeAnalyzer(BarcodeScannerActivity activity)
            {
                this.activityRef = new WeakReference<BarcodeScannerActivity>(activity);
                this.reader = new BarcodeReaderGeneric
                {
                    AutoRotate = false, // Disable for performance - handle rotation differently if needed
                    TryInverted = false, // Disable for performance
                    Options = new ZXing.Common.DecodingOptions
                    {
                        TryHarder = true, 
                        PossibleFormats = new List<BarcodeFormat>
                        {
                            //all formats
                            BarcodeFormat.AZTEC,
                            BarcodeFormat.CODABAR,
                            BarcodeFormat.CODE_39,
                            BarcodeFormat.CODE_93,
                            BarcodeFormat.CODE_128,
                            BarcodeFormat.DATA_MATRIX,
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.ITF,
                            BarcodeFormat.MAXICODE,
                            BarcodeFormat.PDF_417,
                            BarcodeFormat.QR_CODE,
                            BarcodeFormat.RSS_14,
                            BarcodeFormat.RSS_EXPANDED,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E,
                            BarcodeFormat.UPC_EAN_EXTENSION,
                            BarcodeFormat.MSI,
                            BarcodeFormat.PLESSEY,
                            BarcodeFormat.IMB
                        }
                    }
                };
            }

            public void Analyze(IImageProxy imageProxy)
            {
                if (!activityRef.TryGetTarget(out var activity) || !activity.isScanning)
                {
                    imageProxy.Close();
                    return;
                }
                
                // Throttle frame processing
                var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (isProcessing || !activity.isScanning || (currentTime - lastProcessTime) < MIN_FRAME_INTERVAL_MS)
                {
                    imageProxy.Close();
                    return;
                }

                lastProcessTime = currentTime;
                isProcessing = true;

                PlanarYUVLuminanceSource luminanceSource = null;
                try
                {
                    luminanceSource = CreateLuminanceSource(imageProxy);
                    if (luminanceSource != null)
                    {
                        var result = reader.Decode(luminanceSource);
                        if (result != null)
                        {
                            activity.OnBarcodeDetected(result.Text, result.RawBytes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exception if needed
                    Mvx.IoCProvider.Resolve<ILoggerProvider>().GetForType(this.GetType()).Warn("Barcode scanning error", ex);
                    
                }
                finally
                {
                    luminanceSource = null;
                    imageProxy.Close();
                    isProcessing = false;
                }
            }

            private PlanarYUVLuminanceSource CreateLuminanceSource(IImageProxy imageProxy)
            {
                Android.Media.Image image = null;
                Android.Media.Image.Plane[] planes = null;
                Java.Nio.ByteBuffer yBuffer = null;
                try
                {
                    image = imageProxy.Image;
                    if (image == null)
                        return null;

                    planes = image.GetPlanes();
                    if (planes == null || planes.Length == 0)
                        return null;

                    var yPlane = planes[0];
                    yBuffer = yPlane.Buffer;
                    if (yBuffer == null)
                        return null;

                    var width = imageProxy.Width;
                    var height = imageProxy.Height;
                    var rowStride = yPlane.RowStride;
                    var pixelStride = yPlane.PixelStride;
                    
                    // Get Y plane data (luminance)
                    var ySize = yBuffer.Remaining();
                    var yData = new byte[ySize];
                    yBuffer.Get(yData);
                    
                    // Use center crop for better performance - focus on center 80% of image
                    var cropWidth = (int)(width * 0.8);
                    var cropHeight = (int)(height * 0.8);
                    var left = (width - cropWidth) / 2;
                    var top = (height - cropHeight) / 2;
                    
                    // If the Y plane is tightly packed (pixelStride == 1), we can use it directly
                    if (pixelStride == 1)
                    {
                        return new PlanarYUVLuminanceSource(
                            yData,
                            rowStride,
                            height,
                            left,
                            top,
                            cropWidth,
                            cropHeight,
                            false);
                    }
                    else
                    {
                        // Need to repack the data if not tightly packed (rare case)
                        var repackedData = new byte[width * height];
                        for (int row = 0; row < height; row++)
                        {
                            for (int col = 0; col < width; col++)
                            {
                                repackedData[row * width + col] = yData[row * rowStride + col * pixelStride];
                            }
                        }
                        
                        return new PlanarYUVLuminanceSource(
                            repackedData,
                            width,
                            height,
                            left,
                            top,
                            cropWidth,
                            cropHeight,
                            false);
                    }
                }
                catch (Exception ex)
                {
                    Mvx.IoCProvider.Resolve<ILoggerProvider>().GetForType(this.GetType()).Warn("Image conversion error", ex);
                    return null;
                }
                finally
                {
                    yBuffer?.Dispose();
    
                    if (planes != null)
                    {
                        foreach (var plane in planes)
                        {
                            plane?.Dispose();
                        }
                    }
    
                    image?.Dispose();
                }
            }
        }
    }
}
