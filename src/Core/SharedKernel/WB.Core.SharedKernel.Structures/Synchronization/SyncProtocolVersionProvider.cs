namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public static class SyncProtocolVersionProvider
    {
        public static int GetProtocolVersion()
        {
            //should be updated after protocol changes
            //on update please use step 10 or 100
            return 5962;
        }
    }
}
