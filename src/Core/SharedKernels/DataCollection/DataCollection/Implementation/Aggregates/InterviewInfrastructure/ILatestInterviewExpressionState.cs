using WB.Core.SharedKernels.DataCollection.V10;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface ILatestInterviewExpressionState : IInterviewExpressionStateV10
    {
        new ILatestInterviewExpressionState Clone();
    }
}