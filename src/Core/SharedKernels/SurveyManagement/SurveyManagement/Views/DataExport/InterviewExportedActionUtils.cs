using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public static class InterviewExportedActionUtils
    {
        public static InterviewStatus? ConvertToInterviewStatus(this InterviewExportedAction action)
        {
            switch (action)
            {
                case InterviewExportedAction.SupervisorAssigned:
                    return InterviewStatus.SupervisorAssigned;
                case InterviewExportedAction.InterviewerAssigned:
                    return InterviewStatus.InterviewerAssigned;
                case InterviewExportedAction.FirstAnswerSet:
                    return null;
                case InterviewExportedAction.Completed:
                    return InterviewStatus.Completed;
                case InterviewExportedAction.Restarted:
                    return InterviewStatus.Restarted;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return InterviewStatus.ApprovedBySupervisor;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return InterviewStatus.ApprovedByHeadquarters;
                case InterviewExportedAction.RejectedBySupervisor:
                    return InterviewStatus.RejectedBySupervisor;
                case InterviewExportedAction.RejectedByHeadquarter:
                    return InterviewStatus.RejectedByHeadquarters;
                case InterviewExportedAction.Deleted:
                    return InterviewStatus.Deleted;
                case InterviewExportedAction.Restored:
                    return InterviewStatus.Restored;
            }
            return null;
        }
    }
}