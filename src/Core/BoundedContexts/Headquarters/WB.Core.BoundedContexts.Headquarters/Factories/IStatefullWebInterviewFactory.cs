using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IStatefullWebInterviewFactory
    {
        IStatefulInterview Get(string interviewId);
        string GetHumanInterviewId(string interviewId);
        string GetInterviewIdByHumanId(string id);
    }
}