using Android.Content;
using Android.Gms.Nearby.Messages;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    interface INotificationPublisher
    {
        void Init(Context context);

        void Notify(Context context, NotificationModel notification);
        void CreateNotificationChannel(Context context);
    }
}
