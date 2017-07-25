using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewUniqueKeyGenerator
    {
        InterviewKey Get();
    }
}