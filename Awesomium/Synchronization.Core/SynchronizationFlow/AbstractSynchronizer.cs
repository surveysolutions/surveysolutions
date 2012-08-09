using System;
using Synchronization.Core.ClientSettings;

namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        protected ISynchronizer Next;
        protected readonly IClientSettingsProvider ClientSettingsProvider;
        public AbstractSynchronizer(IClientSettingsProvider clientSettingsprovider)
        {
            this.ClientSettingsProvider = clientSettingsprovider;
        }

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
                    throw new SynchronizationException("push wasn't successefull");
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

        public event EventHandler<SynchronizationEvent> PushProgressChanged;
        public event EventHandler<SynchronizationEvent> PullProgressChanged;
        //The event-invoking method that derived classes can override.
        protected virtual void OnPushProgressChanged(SynchronizationEvent e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SynchronizationEvent> handler = PushProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        //The event-invoking method that derived classes can override.
        protected virtual void OnPullProgressChanged(SynchronizationEvent e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SynchronizationEvent> handler = PullProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        protected abstract void ExecutePush();
        protected abstract void ExecutePull();
    }
}
