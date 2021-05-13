using System.Collections.Generic;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public interface IInAppNotificationsCollector
    {
        List<SimpleNotification> CollectInAppNotifications();
    }
}