using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
{
    public class InformationPackageCancellationEventArgs : EventArgs
    {
        public string Reason { get;private set;}

        public InformationPackageCancellationEventArgs(string reason)
        {
            Reason = reason;
        }
    }
}
