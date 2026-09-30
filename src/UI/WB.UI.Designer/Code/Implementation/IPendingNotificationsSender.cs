using System.Threading.Tasks;

namespace WB.UI.Designer.Code.Implementation
{
    /// <summary>
    /// Delivers notifications queued during command processing.
    /// Must be called only after the database transaction has been committed.
    /// </summary>
    public interface IPendingNotificationsSender
    {
        Task SendPendingNotificationsAsync();
        void DiscardPendingNotifications();
    }
}
