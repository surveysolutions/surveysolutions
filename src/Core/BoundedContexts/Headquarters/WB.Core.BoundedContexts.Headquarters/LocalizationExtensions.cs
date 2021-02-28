using System.Web;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class LocalizationExtensions
    {
        public static string ToLocalizeString(this InterviewStatus status)
        {
            return status switch
            {
                InterviewStatus.Created => Strings.InterviewStatus_Created,
                InterviewStatus.SupervisorAssigned => Strings.InterviewStatus_SupervisorAssigned,
                InterviewStatus.Deleted => Strings.InterviewStatus_Deleted,
                InterviewStatus.Restored => Strings.InterviewStatus_Restored,
                InterviewStatus.InterviewerAssigned => Strings.InterviewStatus_InterviewerAssigned,
                InterviewStatus.ReadyForInterview => Strings.InterviewStatus_ReadyForInterview,
                InterviewStatus.SentToCapi => Strings.InterviewStatus_SentToCapi,
                InterviewStatus.Completed => Strings.InterviewStatus_Completed,
                InterviewStatus.Restarted => Strings.InterviewStatus_Restarted,
                InterviewStatus.ApprovedBySupervisor => Strings.InterviewStatus_ApprovedBySupervisor,
                InterviewStatus.RejectedBySupervisor => Strings.InterviewStatus_RejectedBySupervisor,
                InterviewStatus.ApprovedByHeadquarters => Strings.InterviewStatus_ApprovedByHeadquarters,
                InterviewStatus.RejectedByHeadquarters => Strings.InterviewStatus_RejectedByHeadquarters,
                InterviewStatus.WebInterview => Strings.InterviewStatus_WebInterviewRequested,
                _ => string.Empty
            };
        }

        public static string ToLocalizeJavaScriptString(this InterviewStatus status)
        {
            return HttpUtility.JavaScriptStringEncode(ToLocalizeString(status));
        }
    }
}
