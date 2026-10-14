using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IReminderEmailSender
    {
        Task SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireTitle, 
            EmailTextTemplateType reminderType, int thresholdDays);
    }
}
