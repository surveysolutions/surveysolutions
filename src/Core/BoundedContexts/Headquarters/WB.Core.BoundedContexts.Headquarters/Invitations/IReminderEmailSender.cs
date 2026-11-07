using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IReminderEmailSender
    {
        Task<ReminderEmailSendResult> SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle,
            EmailTextTemplateType reminderType, int thresholdDays, Guid? requestId = null);
    }

    public class ReminderEmailSendResult
    {
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int UnconfirmedCount { get; set; }
    }
}
