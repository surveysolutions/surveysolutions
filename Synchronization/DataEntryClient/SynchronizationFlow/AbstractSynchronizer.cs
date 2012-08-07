using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataEntryClient.SynchronizationFlow
{
    public abstract class AbstractSynchronizer
    {
        protected AbstractSynchronizer Next;
        #region Implementation of ISynchronizer

        public AbstractSynchronizer SetNext(AbstractSynchronizer synchronizer)
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

                this.Next.Pull();
            }
        }

        #endregion

        protected abstract void ExecutePush();
        protected abstract void ExecutePull();
    }
}
