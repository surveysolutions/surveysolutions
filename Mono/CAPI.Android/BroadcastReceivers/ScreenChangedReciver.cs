using Android.Content;
using Android.Widget;

namespace CAPI.Android.BroadcastReceivers
{
    [BroadcastReceiver]
    public class ScreenChangedReciver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
        }
    }
}