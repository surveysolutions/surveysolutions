using Cirrious.MvvmCross.Plugins.Sqlite;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public abstract class PlainStorageRow
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}