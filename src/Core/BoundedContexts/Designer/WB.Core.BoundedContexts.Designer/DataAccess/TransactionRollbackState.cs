namespace WB.Core.BoundedContexts.Designer.DataAccess
{
    // Lets a handler veto the commit of the ambient request transaction without throwing, for cases where a
    // failure is reported to the client as a regular response instead of an exception.
    public interface ITransactionRollbackState
    {
        void MarkRollbackOnly();
        bool IsRollbackOnly { get; }
    }

    public class TransactionRollbackState : ITransactionRollbackState
    {
        public bool IsRollbackOnly { get; private set; }

        public void MarkRollbackOnly() => this.IsRollbackOnly = true;
    }
}
