using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        private static Func<bool> isDebugMode;

        //should be updated after protocol changes
        //on release process update please use step 10 or 100
        //to leave space for hot-fixes 
        private const int SyncProtocolVersionNumber = 5962;

        public SyncProtocolVersionProvider(Func<bool> isDebug)
        {
            isDebugMode = isDebug;
        }

        public int? GetProtocolVersion()
        {
            if (isDebugMode())
                return null;

            return SyncProtocolVersionNumber;
        }
    }
}
