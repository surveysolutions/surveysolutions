using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IInterviewUniqueKeyGenerator
    {
        InterviewKey Get();
    }
}