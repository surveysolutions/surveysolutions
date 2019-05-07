using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer.Services
{
    public class InterviewerNotificationsCollector : INotificationsCollector
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IEnumeratorSettings enumeratorSettings;
        public InterviewerNotificationsCollector(IPlainStorage<InterviewView> interviewViewRepository,
            IEnumeratorSettings enumeratorSettings)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.enumeratorSettings = enumeratorSettings;
        }

        public List<NotificationModel> CollectAllNotifications()
        {
            //collect all notifications
            //Sync was not performed for 3 (orange) or 5 (red) days
            //Sync with errors was not retry in 6 hours

            //Completed interview older than 1 day
            //Rejected interview was not opened more that 1,2,3,4,.. days  

            var notifications = new List<NotificationModel>();

            //test
            /*notifications.Add(new NotificationModel()
            {
                NotificationId = (int)NotificationType.Unknown,
                AutoCancel = true,
                ContentTitle = "test",
                ContentText = "test content",
                IconId = Resource.Drawable.icon,
                Intent = GetPendingIntent()
            });*/

            var nowDayBefore = DateTime.Now.Date.AddHours(-24);
            var completedLateCount = this.interviewViewRepository.Count(x => x.CompletedDateTime <= nowDayBefore);

            if (completedLateCount > 0)
            {
                notifications.Add(new NotificationModel()
                {
                    NotificationId = (int)NotificationType.CompletedInterviewsNotSynced,
                    AutoCancel = true,
                    ContentTitle = InterviewerUIResources.Notifications_InterviewsTitle,
                    ContentText = InterviewerUIResources.Notifications_CompletedInterviewsText.FormatString(completedLateCount),
                    IconId = Resource.Drawable.icon,
                    Intent = GetPendingIntent()
                });
            }

            var UTCDayBeforeNow = DateTime.UtcNow.Date.AddHours(-24);
            var rejectedNotOpenedCount = 
                this.interviewViewRepository.Count(x => x.Status == WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor && 
                                                        x.FromHqSyncDateTime <= UTCDayBeforeNow);

            if (rejectedNotOpenedCount > 0)
            {
                notifications.Add(new NotificationModel()
                {
                    NotificationId = (int)NotificationType.RejectedInterviewNotOpened,
                    AutoCancel = true,
                    ContentTitle = InterviewerUIResources.Notifications_InterviewsTitle,
                    ContentText = InterviewerUIResources.Notifications_CompletedInterviewsText.FormatString(completedLateCount),
                    IconId = Resource.Drawable.icon,
                    Intent = GetPendingIntent()
                });
            }


            if (this.enumeratorSettings.LastSync.HasValue)
            {
                if (this.enumeratorSettings.LastSyncSucceeded == false)
                {
                    if (this.enumeratorSettings.LastSync.Value <= DateTime.Now.Date.AddHours(-6))
                    {
                        notifications.Add(new NotificationModel()
                        {
                            NotificationId = (int)NotificationType.NoRetryAfterFailedSync,
                            AutoCancel = true,
                            ContentTitle = InterviewerUIResources.Notifications_SyncTitle,
                            ContentText = InterviewerUIResources.Notifications_SyncNoRetryTitle,
                            IconId = Resource.Drawable.icon,
                            Intent = GetPendingIntent()
                        });
                    }
                }
                else
                {
                    if (this.enumeratorSettings.LastSync.Value <= DateTime.Now.Date.AddHours(-6))
                    {
                        notifications.Add(new NotificationModel()
                        {
                            NotificationId = (int)NotificationType.LastSyncTooLongAgo,
                            AutoCancel = true,
                            ContentTitle = InterviewerUIResources.Notifications_SyncTitle,
                            ContentText = InterviewerUIResources.Notifications_SyncTooLongAgoText,
                            IconId = Resource.Drawable.icon,
                            Intent = GetPendingIntent()
                        });
                    }
                }
            }

            return notifications;
        }

        private PendingIntent GetPendingIntent()
        {
            Intent notificationIntent = Application.Context.PackageManager.GetLaunchIntentForPackage(Application.Context.PackageName);
            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, 
                notificationIntent, 0);

            return pendingIntent;
        }
    }
}
