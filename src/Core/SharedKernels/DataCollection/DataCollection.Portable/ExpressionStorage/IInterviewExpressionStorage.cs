namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewExpressionStorage
    {
        void Initialize(IInterviewState state);

        IInterviewLevel GetLevel(Identity rosterIdentity);
    }
}