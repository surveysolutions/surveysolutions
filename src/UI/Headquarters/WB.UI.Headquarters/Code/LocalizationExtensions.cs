﻿using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public static class LocalizationExtensions
    {
        public static string ToLocalizeString(this InterviewStatus status)
        {
            string returnValue = string.Empty;
            switch (status)
            {
                    case InterviewStatus.Created:
                    returnValue = Strings.InterviewStatus_Created;
                    break;
                    case InterviewStatus.SupervisorAssigned:
                    returnValue = Strings.InterviewStatus_SupervisorAssigned;
                    break;
                    case InterviewStatus.Deleted:
                    returnValue = Strings.InterviewStatus_Deleted;
                    break;
                    case InterviewStatus.Restored:
                    returnValue = Strings.InterviewStatus_Restored;
                    break;
                    case InterviewStatus.InterviewerAssigned:
                    returnValue = Strings.InterviewStatus_InterviewerAssigned;
                    break;
                    case InterviewStatus.ReadyForInterview:
                    returnValue = Strings.InterviewStatus_ReadyForInterview;
                    break;
                    case InterviewStatus.SentToCapi:
                    returnValue = Strings.InterviewStatus_SentToCapi;
                    break;
                    case InterviewStatus.Completed:
                    returnValue = Strings.InterviewStatus_Completed;
                    break;
                    case  InterviewStatus.Restarted:
                    returnValue = Strings.InterviewStatus_Restarted;
                    break;
                    case InterviewStatus.ApprovedBySupervisor:
                    returnValue = Strings.InterviewStatus_ApprovedBySupervisor;
                    break;
                    case InterviewStatus.RejectedBySupervisor:
                    returnValue = Strings.InterviewStatus_RejectedBySupervisor;
                    break;
                    case InterviewStatus.ApprovedByHeadquarters:
                    returnValue = Strings.InterviewStatus_ApprovedByHeadquarters;
                    break;
                    case InterviewStatus.RejectedByHeadquarters:
                    returnValue = Strings.InterviewStatus_RejectedByHeadquarters;
                    break;
            }
            return returnValue;
        }
    }
}