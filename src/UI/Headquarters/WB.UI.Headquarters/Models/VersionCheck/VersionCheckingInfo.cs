namespace WB.UI.Headquarters.Models.VersionCheck
{
    public class VersionCheckingInfo
    {
        public static readonly string StorageKey = "versoinKey";

        public int Build { set; get; }
        public string Version { set; get; }
        public string VersionString { set; get; }
    }
}