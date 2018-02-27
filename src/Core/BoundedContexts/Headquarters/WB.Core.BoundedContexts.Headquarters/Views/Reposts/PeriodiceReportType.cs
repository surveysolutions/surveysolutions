namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public enum PeriodiceReportType
    {
        NumberOfCompletedInterviews = 0,
        NumberOfInterviewTransactionsBySupervisor = 1,
        NumberOfInterviewTransactionsByHQ = 2,
        NumberOfInterviewsApprovedByHQ = 3,
        
        AverageCaseAssignmentDuration = 4,
        AverageInterviewDuration = 5,
        AverageSupervisorProcessingTime = 6,
        AverageHQProcessingTime = 7,
        AverageOverallCaseProcessingTime = 8
    }
}