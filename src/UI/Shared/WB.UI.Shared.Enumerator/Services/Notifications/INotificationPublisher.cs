using System.Collections.Generic;
using Android.Content;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public interface INotificationPublisher
    {
        void Init(Context context);

        void Notify(Context context, NotificationModel notification);

        void Notify(Context context, List<NotificationModel> notificationModels, bool renderSummary);
        void CreateNotificationChannel(Context context);

        void CancelAllNotifications(Context context);
    }
}
