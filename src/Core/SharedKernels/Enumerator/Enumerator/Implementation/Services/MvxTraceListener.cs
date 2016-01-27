using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using SQLite.Net;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class MvxTraceListener : ITraceListener
    {
        public void Receive(string message)
        {
#if DEBUG
            if (message.Contains("Executing Query"))
            {
                Mvx.TaggedTrace(MvxTraceLevel.Diagnostic, "SQLite", message);
            }
#endif
        }
    }
}