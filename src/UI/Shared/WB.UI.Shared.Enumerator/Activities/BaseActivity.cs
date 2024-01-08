using Android.Animation;
using Android.Views;
using AndroidX.VectorDrawable.Graphics.Drawable;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using NLog;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxActivityPresentation]
    public abstract class BaseActivity<TViewModel> : MvxActivity<TViewModel> where TViewModel : class, IMvxViewModel
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GlobalRegistry.Remove(this.activityId);
            }

            base.Dispose(disposing);
        }

        private readonly Guid activityId = Guid.NewGuid();
        protected abstract int ViewResourceId { get; }
        private Logger log;
        private OnBackPressedCallbackWrapper backPressedCallbackWrapper;

        private static Dictionary<Guid, string> GlobalRegistry { get; } = new();

        protected override void OnCreate(Bundle bundle)
        {
            GlobalRegistry.Add(this.activityId, this.GetType().Name + $" at {DateTime.Now}" );
            
            log = LogManager.GetLogger(this.GetType().Name);
            log.Trace("Create");
            base.OnCreate(bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);

            
            if (BackButtonCustomAction)
            {
                backPressedCallbackWrapper = new OnBackPressedCallbackWrapper(BackButtonPressed);
                OnBackPressedDispatcher.AddCallback(this, backPressedCallbackWrapper);
            }
        }
        
        protected override void OnStart()
        {
            log.Trace("Start");
            base.OnStart();
        }

        protected override void OnResume()
        {
            log.Trace("Resume");
            var messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            messenger.Publish(new ApplicationResumeMessage(this));
            base.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            log.Trace($"OnRequestPermissionsResult permissions {string.Join(',', permissions)} grantResults {string.Join(',', grantResults)}");
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning disable CA1416 // Validate platform compatibility
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        protected override void OnPause()
        {
            log.Trace("Pause");
            base.OnPause();
        }

        protected override void OnStop()
        {
            log.Trace("Stop");
            base.OnStop();
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }

        public override void OnLowMemory()
        {
            this.TryWriteMemoryInformationToLog("LowMemory notification");
            base.OnLowMemory();
        }

        protected virtual bool BackButtonCustomAction => false;

        protected virtual void BackButtonPressed()
        {
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // get current instance of ObjectAnimator and call RemoveAllListeners from ObjectAnimator to prevent memory leaks
            //var currentAnimator = ObjectAnimator.OfFloat(this, "alpha", 1f, 1f);
            //currentAnimator?.RemoveAllListeners();
            
            
            
            backPressedCallbackWrapper?.Remove();
            backPressedCallbackWrapper?.Dispose();
            backPressedCallbackWrapper = null;

            TryWriteMemoryInformationToLog($"Destroyed Activity {this.GetType().Name}");
            this.BindingContext?.ClearAllBindings();
            this.ViewModel?.DisposeIfDisposable();
            
            //cleanup cache to remove disposed viewmodel
            if (Mvx.IoCProvider.TryResolve<IMvxSingleViewModelCache>(out var cache))
            {
                cache?.GetAndClear(null);
            }
        }

        protected void SetMenuItemIcon(IMenu menu, int itemId, int drawableId)
        {
            var item = menu.FindItem(itemId);
            var drawable = VectorDrawableCompat.Create(Resources, drawableId, Theme);
            item.SetIcon(drawable);
        }

        private void TryWriteMemoryInformationToLog(string message)
        {
            try
            {
                if (Mvx.IoCProvider.TryResolve<IEnvironmentInformationUtils>(out var utils))
                {
                    log.Info($"{message} RAM: {utils.GetRAMInformation()} Disk: {utils.GetDiskInformation()}");
                    return;
                }
            }
            catch
            {
                // ignore if we cannot get info about RAM and Disk
            }
        }
    }
}
