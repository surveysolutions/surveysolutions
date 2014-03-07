namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public enum InterviewStatus
    {
        Created = 20,
        SupervisorAssigned = 40,
        Deleted = -1,
        Restored = 0,
        InterviewerAssigned = 60,
        ReadyForInterview = 80,
        SentToCapi = 85,
        Completed = 100,
        Restarted = 95,

        ApprovedBySupervisor = 120,
        RejectedBySupervisor = 65,

        RejectedByHeadquarters = 125,
        ApprovedByHeadquarters = 130
    }
}
