using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        private const int SyncProtocolVersionNumber = 5962;

        public int GetProtocolVersion()
        {
            return SyncProtocolVersionNumber;
        }
    }
}
