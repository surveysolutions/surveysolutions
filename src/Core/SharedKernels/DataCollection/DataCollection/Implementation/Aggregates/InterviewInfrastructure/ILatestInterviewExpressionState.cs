using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface ILatestInterviewExpressionState : IInterviewExpressionStateV8
    {
        new ILatestInterviewExpressionState Clone();
    }
}