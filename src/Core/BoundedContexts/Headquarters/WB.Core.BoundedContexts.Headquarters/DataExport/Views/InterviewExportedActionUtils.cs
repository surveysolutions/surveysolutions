using WB.ServicesIntegration.Export;
using InterviewStatus = WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
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
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return InterviewStatus.ApprovedBySupervisor;
                case InterviewExportedAction.Created:
                    return InterviewStatus.Created;
            }
            return null;
        }
    }
}