using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class NotificationPublisher : INotificationPublisher
    {
        static readonly string CHANNEL_ID = "interviewer_notification";
        static readonly string CHANNEL_NAME = "General notifications";
        static readonly string CHANNEL_DESCRIPTION = "Main channel for notifications";
        static readonly string GROUP_NAME = "Interviewer_group";
        private static readonly int SUMMARY_ID = 1000001;

        public void Init(Context context)
        {
            CreateNotificationChannel(context);
        }

        public void Notify(Context context, NotificationModel notificationModel)
        {
            // Build the notification:
            Notification notification = this.GetNotification(context, notificationModel);

            // Get the notification manager:
            NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(notificationModel.NotificationId, notification);
        }

        private Notification GetNotification(Context context, NotificationModel notificationModel)
        {
            // Instantiate the builder and set notification elements:
            NotificationCompat.Builder builder =
                    new NotificationCompat.Builder(context, CHANNEL_ID)
                        .SetAutoCancel(notificationModel.AutoCancel)

                        .SetContentIntent(notificationModel.Intent)
                        .SetContentTitle(notificationModel.ContentTitle)
                        .SetContentText(notificationModel.ContentText)
                        .SetGroup(GROUP_NAME)
                        .SetSmallIcon(notificationModel.IconId) //replace this icon
                //there are difference if android.os.Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP
                //icon should be transparent
                ;

            // Build the notification:
            return builder.Build();
        }

        public void Notify(Context context, List<NotificationModel> notificationModels, bool renderSummary)
        {
            foreach (var notificationModel in notificationModels)
            {
                this.Notify(context, notificationModel);
            }
            
            if (notificationModels.Count > 1 && renderSummary)
            {
                NotificationCompat.Builder builder =
                        new NotificationCompat.Builder(context, CHANNEL_ID)
                            
                            .SetContentTitle("Summary")
                            .SetContentText("{notificationModels.Count} notifications")

                            .SetGroup(GROUP_NAME)
                            .SetGroupSummary(true)
                            .SetSmallIcon(notificationModels.First().IconId);

                // Build the notification:
                var summary =  builder.Build();

                NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager?.Notify(SUMMARY_ID, summary);
            }
        }

        public void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                // Channels are visible in application settings
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.Default)
            {
                Description = CHANNEL_DESCRIPTION
            };

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
