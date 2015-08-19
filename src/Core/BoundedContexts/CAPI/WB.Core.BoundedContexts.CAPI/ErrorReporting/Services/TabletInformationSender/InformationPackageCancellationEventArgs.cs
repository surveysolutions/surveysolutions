using System;

namespace WB.Core.BoundedContexts.Capi.ErrorReporting.Services.TabletInformationSender
{
    public class InformationPackageCancellationEventArgs : EventArgs
    {
        public string Reason { get;private set;}

        public InformationPackageCancellationEventArgs(string reason)
        {
            this.Reason = reason;
        }
    }
}
