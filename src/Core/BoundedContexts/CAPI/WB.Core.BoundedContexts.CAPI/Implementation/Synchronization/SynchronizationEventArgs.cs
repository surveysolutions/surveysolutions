using System;

namespace WB.Core.BoundedContexts.Capi.Implementation.Synchronization
{
    public class SynchronizationEventArgs:EventArgs
    {
        public SynchronizationEventArgs(string operationTitle, Operation operationType, bool cancelable)
        {
            this.OperationTitle = operationTitle;
            this.OperationType = operationType;
            this.Cancelable = cancelable;
        }

        public string OperationTitle { get; private set; }
        public Operation OperationType { get; private set; }
        public bool Cancelable { get; private set; }
    }

    public enum Operation
    {
        Handshake,
        Pull,
        Push,
        Validation
    }
}