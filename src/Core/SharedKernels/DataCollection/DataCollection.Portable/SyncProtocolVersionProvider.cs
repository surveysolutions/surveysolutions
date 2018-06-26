namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        public static readonly int ProtectedVariablesIntroduced = 7070;

        //previous values: 5962, 7018, 7034, 7050, 7060, 7070 - where KP-11462 was introduced 
        public int GetProtocolVersion() => 7080;

        public int GetLastNonUpdatableVersion() => 7000;
    }
}
