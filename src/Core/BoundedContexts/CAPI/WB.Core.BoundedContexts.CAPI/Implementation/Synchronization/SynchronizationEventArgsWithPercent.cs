using System;

namespace WB.Core.BoundedContexts.Capi.Implementation.Synchronization
{
    public class SynchronizationEventArgsWithPercent:SynchronizationEventArgs
    {
        public SynchronizationEventArgsWithPercent(string operationTitle, Operation operationType, bool cancelable, int percent)
            : base(operationTitle, operationType,cancelable)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentException("percent value is incorrect");
            this.Percent = percent;
            
        }

        public int Percent { get; private set; }
    }
}