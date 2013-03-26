// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncBroadcastReceiver.cs" company="">
//   
// </copyright>
// <summary>
//   The sync broadcast receiver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidApp.BroadcastReceivers
{
    using Android.App;
    using Android.Content;

    using AndroidApp.Services;

    /// <summary>
    /// The sync broadcast receiver.
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new[] { SyncService.SyncFinishededAction }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class SyncBroadcastReceiver : BroadcastReceiver
    {
        #region Public Methods and Operators

        /// <summary>
        /// The on receive.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="intent">
        /// The intent.
        /// </param>
        public override void OnReceive(Context context, Intent intent)
        {
            var nMgr = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var notification = new Notification(Resource.Drawable.capi, "Sync is finished");
            PendingIntent pendingIntent = PendingIntent.GetActivity(
                context, 0, new Intent(context, typeof(SyncActivity)), 0);
            notification.SetLatestEventInfo(context, "Sync's done", "Done", pendingIntent);
            nMgr.Notify(0, notification);

            // Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
        }

        #endregion
    }
}