using Cirrious.MvvmCross.Plugins.Sqlite;

namespace WB.UI.Capi.Syncronization.Implementation
{
    public class SyncPackageId
    {
        [PrimaryKey]
        public string PackageId { get; set; }

        public long SortIndex { get; set; }

        public string Type { get; set; }

        public string UserId { get; set; }
    }
}