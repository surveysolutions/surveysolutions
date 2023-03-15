using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Extensions.Extensions;

public static class LocalizeExtensions
{
    public static string ToLocalizeString(this InterviewStatus status)
    {
        string returnValue = string.Empty;
        switch (status)
        {
            case InterviewStatus.Created:
                returnValue = UIResources.InterviewStatus_Created;
                break;
            case InterviewStatus.SupervisorAssigned:
                returnValue = UIResources.InterviewStatus_SupervisorAssigned;
                break;
            case InterviewStatus.Deleted:
                returnValue = UIResources.InterviewStatus_Deleted;
                break;
            case InterviewStatus.Restored:
                returnValue = UIResources.InterviewStatus_Restored;
                break;
            case InterviewStatus.InterviewerAssigned:
                returnValue = UIResources.InterviewStatus_InterviewerAssigned;
                break;
            case InterviewStatus.Completed:
                returnValue = UIResources.InterviewStatus_Completed;
                break;
            case InterviewStatus.Restarted:
                returnValue = UIResources.InterviewStatus_Restarted;
                break;
            case InterviewStatus.ApprovedBySupervisor:
                returnValue = UIResources.InterviewStatus_ApprovedBySupervisor;
                break;
            case InterviewStatus.RejectedBySupervisor:
                returnValue = UIResources.InterviewStatus_RejectedBySupervisor;
                break;
            case InterviewStatus.ApprovedByHeadquarters:
                returnValue = UIResources.InterviewStatus_ApprovedByHeadquarters;
                break;
            case InterviewStatus.RejectedByHeadquarters:
                returnValue = UIResources.InterviewStatus_RejectedByHeadquarters;
                break;
        }

        return returnValue;
    }
}