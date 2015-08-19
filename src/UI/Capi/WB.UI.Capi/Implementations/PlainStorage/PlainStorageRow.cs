using Cirrious.MvvmCross.Plugins.Sqlite;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public class PlainStorageRow
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string SerializedData { get; set; }
    }
}