using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface ILatestInterviewExpressionState: IInterviewExpressionStateV7
    {
        new ILatestInterviewExpressionState Clone();
    }
}