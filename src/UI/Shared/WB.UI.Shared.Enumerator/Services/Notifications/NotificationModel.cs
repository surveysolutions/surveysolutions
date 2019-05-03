using Android.App;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class NotificationModel
    {
        public string ContentTitle { set; get; }
        public string ContentText { set; get; }
        public bool AutoCancel { set; get; } = true;

        public int? NotificationId { set; get; }


        public int IconId { set; get; }
        public PendingIntent Intent { set; get; }
    }
}
