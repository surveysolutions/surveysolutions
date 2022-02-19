using Microsoft.AppCenter.Crashes;
using Xamarin.ANRWatchDog;

namespace WB.UI.Shared.Enumerator.Utils
{
    public class ANRListener : ANRWatchDog.IANRListener
    {
        public void OnAppNotResponding(ANRError error)
        {
            Crashes.TrackError(error);
        }
    }
}
