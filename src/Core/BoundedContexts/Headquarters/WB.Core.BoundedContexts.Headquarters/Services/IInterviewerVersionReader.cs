namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IInterviewerVersionReader
    {
        int? InterviewerBuildNumber { get; }
        int? SupervisorBuildNumber { get; }
        string InterviewerVersionString { get; }
        string SupervisorVersionString { get; }
    }
}
