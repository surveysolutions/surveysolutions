using System.Collections.Generic;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public interface INotificationsCollector
    {
        List<NotificationModel> CollectAllNotifications();
    }
}
