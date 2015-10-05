using Cirrious.MvvmCross.Plugins.Sqlite;

namespace WB.UI.Interviewer.Implementations.PlainStorage
{
    public class PlainStorageRow
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string SerializedData { get; set; }
    }
}