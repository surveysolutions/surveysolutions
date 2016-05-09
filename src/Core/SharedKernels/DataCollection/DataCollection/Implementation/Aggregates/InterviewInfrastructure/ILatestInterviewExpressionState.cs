using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface ILatestInterviewExpressionState : IInterviewExpressionStateV9
    {
        new ILatestInterviewExpressionState Clone();
    }
}