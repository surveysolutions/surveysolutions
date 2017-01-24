using Android.Content;
using Android.OS;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class VibrationService : IVirbationService
    {
        private readonly IMvxAndroidCurrentTopActivity currentTopActivity;

        public VibrationService(IMvxAndroidCurrentTopActivity currentTopActivity)
        {
            this.currentTopActivity = currentTopActivity;
        }

        public void Vibrate()
        {
            var vibrator = (Vibrator) this.currentTopActivity?.Activity?.GetSystemService(Context.VibratorService);
            if(vibrator == null || !vibrator.HasVibrator) return;

            vibrator.Vibrate(100);
        }
    }
}