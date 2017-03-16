using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal interface IInterviewUniqueKeyGenerator
    {
        InterviewKey Get();
    }
}