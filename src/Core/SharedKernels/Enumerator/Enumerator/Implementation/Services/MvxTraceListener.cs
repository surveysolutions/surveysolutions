using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using SQLite.Net;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class MvxTraceListener : ITraceListener
    {
        private readonly string traceCategory;

        public MvxTraceListener(string traceCategory)
        {
            this.traceCategory = traceCategory;
        }

        public void Receive(string message)
        {
#if DEBUG
            if (message.Contains("Executing Query"))
            {
                Mvx.TaggedTrace(MvxTraceLevel.Diagnostic, this.traceCategory, message);
            }
#endif
        }
    }
}