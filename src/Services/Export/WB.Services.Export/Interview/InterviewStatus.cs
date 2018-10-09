namespace WB.Services.Export.Interview
{
    public enum InterviewStatus
    {
        Deleted = -1,
        Restored = 0,

        Created = 20,
        SupervisorAssigned = 40,
        InterviewerAssigned = 60,
        ReadyForInterview = 80,
        SentToCapi = 85,
        Restarted = 95,
        Completed = 100,

        RejectedBySupervisor = 65,
        ApprovedBySupervisor = 120,

        RejectedByHeadquarters = 125,
        ApprovedByHeadquarters = 130
    }
}
