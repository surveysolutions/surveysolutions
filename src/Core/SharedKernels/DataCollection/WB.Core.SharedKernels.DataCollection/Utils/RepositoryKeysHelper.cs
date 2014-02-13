namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class RepositoryKeysHelper
    {
        public static string GetVersionedKey(string id, long version)
        {
            return string.Format("{0}-{1}", id, version);
        }
    }
}