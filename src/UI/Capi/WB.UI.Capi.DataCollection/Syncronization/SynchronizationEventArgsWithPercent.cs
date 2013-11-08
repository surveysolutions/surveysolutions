using System;

namespace WB.UI.Capi.DataCollection.Syncronization
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