using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SynchronizationContext
    {
        private List<string> synchronizationMessages = new List<string>();
        private List<string> synchronizationErrors = new List<string>(); 

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
            synchronizationMessages.Clear();
            synchronizationErrors.Clear();
            this.PushMessage("Synchronization started");
        }

        public void Stop()
        {
            IsRunning = false;
            this.PushMessage("Synchronization done");
        }

        public void PushMessage(string message)
        {
            synchronizationMessages.Add(message);
        }

        public void PushError(string message)
        {
            synchronizationErrors.Add(message);
        }

        public string GetStatus()
        {
            return synchronizationMessages.LastOrDefault();
        }

        public IReadOnlyCollection<string> GetMessages()
        {
            return new ReadOnlyCollection<string>(this.synchronizationMessages);
        }

        public IReadOnlyCollection<string> GetErrors()
        {
            return new ReadOnlyCollection<string>(this.synchronizationErrors);
        }
    }
}