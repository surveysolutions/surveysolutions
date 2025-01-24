using Android.Content;
using Android.OS;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class VibrationService : IVibrationService
    {
        private readonly IMvxAndroidCurrentTopActivity currentTopActivity;

        public VibrationService()
        {
            this.currentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
        }

        public void Vibrate()
        {
            var vibrator = (Vibrator) this.currentTopActivity?.Activity?.GetSystemService(Context.VibratorService);
            if(this.isDisabled || vibrator == null || !vibrator.HasVibrator) return;

            vibrator.Vibrate(100);
        }

        private bool isDisabled;
        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;
    }
}
