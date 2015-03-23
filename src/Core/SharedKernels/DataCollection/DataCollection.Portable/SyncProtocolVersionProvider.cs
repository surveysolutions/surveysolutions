using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        //previous value 5962
        private const int SyncProtocolVersionNumber = 7000;


        private const int NonUpdatableShiftVersionNumber = 7000;

        public int GetProtocolVersion()
        {
            return SyncProtocolVersionNumber;
        }

        public int GetLastNonUpdatableVersion()
        {
            return NonUpdatableShiftVersionNumber;
        }
    }
}
