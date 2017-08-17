namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public enum InterviewExportedAction
    {
        SupervisorAssigned = 0,
        InterviewerAssigned = 1,
        FirstAnswerSet = 2,
        Completed = 3,
        Restarted = 4,
        ApprovedBySupervisor = 5,
        ApprovedByHeadquarter = 6,
        RejectedBySupervisor = 7,
        RejectedByHeadquarter = 8,
        Deleted = 9,
        Restored = 10,
        UnapprovedByHeadquarter = 11,
        Created = 12
    }
}