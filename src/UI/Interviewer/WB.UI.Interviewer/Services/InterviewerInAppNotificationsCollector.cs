using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer.Services
{
    public class InterviewerInAppNotificationsCollector : IInAppNotificationsCollector
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;

        public InterviewerInAppNotificationsCollector(
            IPlainStorage<InterviewView> interviewViewRepository,
            IAssignmentDocumentsStorage assignmentsRepository)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.assignmentsRepository = assignmentsRepository;
        }

        public List<SimpleNotification> CollectInAppNotifications()
        {
            var notifications = new List<SimpleNotification>();

            // Assignment has not been touched for more than 3 days (no new interviews started from that assignment);
            // More than 5 started interviews.
            // More than 10 completed interviews.

            //vague condition
            //skipping for now
            //Distance between the earliest and latest started and not finished interviews is more than 5 days,
            //or any single started interview is older than 5 days.

            
            //test
            /*notifications.Add(new SimpleNotification()
            {
                ContentText = "test content"
            });*/


            var startedInterviewsCount = this.interviewViewRepository.Count(x => x.Status == InterviewStatus.Restarted 
                                                                                 || x.Status == InterviewStatus.Created 
                                                                                 || x.Status == InterviewStatus.InterviewerAssigned);
            if (startedInterviewsCount > 5)
            {
                notifications.Add(new SimpleNotification()
                {
                    ContentText = EnumeratorUIResources.Notifications_ManyStartedInterviews
                });
            }

            var completedInterviewsCount = this.interviewViewRepository.Count(x => x.Status == InterviewStatus.Completed);
            if(completedInterviewsCount > 10)
            {
                notifications.Add(new SimpleNotification()
                {
                    ContentText = EnumeratorUIResources.Notifications_ManyCompletedInterviews
                });
            }

            var threeDaysBeforeNow = DateTime.Now.Date.AddHours(-24 * 3);

            var untouchedAssignmentsCount = this.assignmentsRepository.Count(x => x.LastUpdated != null && x.LastUpdated < threeDaysBeforeNow);
            if (untouchedAssignmentsCount > 0)
            {
                notifications.Add(new SimpleNotification()
                {
                    ContentText = EnumeratorUIResources.Notifications_StaleAssignments
                });
            }

            return notifications;
        }
    }
}
