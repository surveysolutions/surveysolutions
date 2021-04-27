using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer.Services
{
    public class InterviewerNotificationsCollector : INotificationsCollector
    {
        private readonly IWorkspaceService workspaceService;
        private readonly IInScopeExecutor inScopeExecutor;

        public InterviewerNotificationsCollector(IWorkspaceService workspaceService,
            IInScopeExecutor inScopeExecutor)
        {
            this.workspaceService = workspaceService;
            this.inScopeExecutor = inScopeExecutor;
        }

        public List<NotificationModel> CollectAllNotifications()
        {
            //collect all notifications
            //Sync was not performed for 3 (orange) or 5 (red) days
            //Sync with errors was not retry in 6 hours

            //Completed interview older than 1 day
            //Rejected interview was not opened more that 1,2,3,4,.. days  

            var notifications = new List<NotificationModel>();

            var workspaces = workspaceService.GetAll();

            foreach (var workspace in workspaces)
            {
                inScopeExecutor.Execute(serviceLocator =>
                {
                    var interviewViewRepository = serviceLocator.GetInstance<IPlainStorage<InterviewView>>();
                    var enumeratorSettings = serviceLocator.GetInstance<IEnumeratorSettings>();

                    //test
        /*            notifications.Add(new NotificationModel()
                    {
                        NotificationId = 10021,
                        AutoCancel = true,
                        ContentTitle = "test",
                        ContentText = "test content",
                        IconId = Resource.Drawable.icon,
                        Intent = GetPendingIntent()
                    });

                    notifications.Add(new NotificationModel()
                    {
                        NotificationId = 10022,
                        AutoCancel = true,
                        ContentTitle = "test 1",
                        ContentText = "test content 1",
                        IconId = Resource.Drawable.icon,
                        Intent = GetPendingIntent()
                    });*/

                    var oneDayBeforeNow = DateTime.Now.Date.AddHours(-24);
                    var completedLateCount = interviewViewRepository.Count(x => x.CompletedDateTime <= oneDayBeforeNow 
                                                                                     && x.Status == InterviewStatus.Completed);
                    if (completedLateCount > 0)
                    {
                        notifications.Add(new NotificationModel()
                        {
                            NotificationId = (int)NotificationType.CompletedInterviewsNotSynced,
                            AutoCancel = true,
                            ContentTitle = EnumeratorUIResources.Notifications_InterviewsTitle,
                            ContentText = EnumeratorUIResources.Notifications_CompletedInterviewsText.FormatString(completedLateCount),
                            IconId = Resource.Drawable.icon,
                            Intent = GetPendingIntent()
                        });
                    }

                    var UTCDayBeforeNow = DateTime.UtcNow.Date.AddHours(-24);
                    var rejectedNotOpenedCount = 
                        interviewViewRepository.Count(x => x.Status == InterviewStatus.RejectedBySupervisor && 
                                                                x.FromHqSyncDateTime <= UTCDayBeforeNow);

                    if (rejectedNotOpenedCount > 0)
                    {
                        notifications.Add(new NotificationModel()
                        {
                            NotificationId = (int)NotificationType.RejectedInterviewNotOpened,
                            AutoCancel = true,
                            ContentTitle = EnumeratorUIResources.Notifications_InterviewsTitle,
                            ContentText = EnumeratorUIResources.Notifications_RejectedInterviewsText.FormatString(rejectedNotOpenedCount),
                            IconId = Resource.Drawable.icon,
                            Intent = GetPendingIntent()
                        });
                    }

                    if (enumeratorSettings.LastSync.HasValue)
                    {
                        if (enumeratorSettings.LastSyncSucceeded == false)
                        {
                            if (enumeratorSettings.LastSync.Value <= DateTime.Now.Date.AddHours(-6))
                            {
                                notifications.Add(new NotificationModel()
                                {
                                    NotificationId = (int)NotificationType.NoRetryAfterFailedSync,
                                    AutoCancel = true,
                                    ContentTitle = EnumeratorUIResources.Notifications_SyncTitle,
                                    ContentText = EnumeratorUIResources.Notifications_SyncNoRetryTitle,
                                    IconId = Resource.Drawable.icon,
                                    Intent = GetPendingIntent()
                                });
                            }
                        }
                        else
                        {
                            if (enumeratorSettings.LastSync.Value <= DateTime.Now.Date.AddHours(-24 * 4))
                            {
                                notifications.Add(new NotificationModel()
                                {
                                    NotificationId = (int)NotificationType.LastSyncTooLongAgo,
                                    AutoCancel = true,
                                    ContentTitle = EnumeratorUIResources.Notifications_SyncTitle,
                                    ContentText = EnumeratorUIResources.Notifications_SyncTooLongAgoText,
                                    IconId = Resource.Drawable.icon,
                                    Intent = GetPendingIntent()
                                });
                            }
                        }
                    }

                }, workspace.Name);
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
