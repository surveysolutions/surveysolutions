using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        //previous values: 5962, 7018
        private const int SyncProtocolVersionNumber = 7034;


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
