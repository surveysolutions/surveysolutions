namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        protected ISynchronizer Next;
        #region Implementation of ISynchronizer

        public ISynchronizer SetNext(ISynchronizer synchronizer)
        {
            Next = synchronizer;
            return synchronizer;
        }

        public void Push()
        {
            try
            {
                ExecutePush();
            }
            catch (SynchronizationException)
            {
                if (this.Next == null)
                    new SynchronizationException("push wasn't successefull");
                this.Next.Push();

            }
        }

        public void Pull()
        {
            try
            {
                ExecutePull();
            }
            catch (SynchronizationException)
            {
                if (this.Next == null)
                    new SynchronizationException("push wasn't successefull");
                this.Next.Pull();
            }
        }

        #endregion

        protected abstract void ExecutePush();
        protected abstract void ExecutePull();
    }
}
