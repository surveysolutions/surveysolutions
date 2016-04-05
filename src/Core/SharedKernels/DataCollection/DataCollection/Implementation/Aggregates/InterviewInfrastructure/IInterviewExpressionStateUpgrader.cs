namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface IInterviewExpressionStateUpgrader
    {
        ILatestInterviewExpressionState UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state);
    }
}
