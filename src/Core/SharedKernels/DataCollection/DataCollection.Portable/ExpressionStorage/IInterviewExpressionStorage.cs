namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewExpressionStorage
    {
        void Initialize(IInterviewStateForExpressions state);

        IInterviewLevel GetLevel(Identity rosterIdentity);
    }
}