using System;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using MvvmCross;


namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    [Service(Permission = "android.permission.BIND_JOB_SERVICE", Exported = true)]
    public class NotificationJobIntentService : JobIntentService
    {
        private static int MY_JOB_ID = 1001;

        public static void EnqueueWork(Context context, Intent work)
        {
            Java.Lang.Class cls = Java.Lang.Class.FromType(typeof(NotificationJobIntentService));
            try
            {
                EnqueueWork(context, cls, MY_JOB_ID, work);
            }
            catch (Exception ex)
            {
                //log exceptions
            }
        }

        protected override void OnHandleWork(Intent p0)
        {
            var collector = Mvx.IoCProvider.Resolve<INotificationsCollector>();
            var publisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();

            var notificationsToSend = collector.CollectAllNotifications();

            var context = this;

            publisher.Init(context);

            foreach (var notificationModel in notificationsToSend)
            {
                publisher.Notify(context, notificationModel);
            }
        }
    }

}
