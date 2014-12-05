using Cirrious.MvvmCross.Plugins.Sqlite;
using SQLite;

namespace AndroidNcqrs.Eventing.Storage.SQLite.Tests
{
    internal class SqlFactoryMock : ISQLiteConnectionFactory
    {
        public SqlFactoryMock(string dbPath)
        {
            this.sqlConnection = new SQLiteConnection(dbPath);
        }

        ISQLiteConnection sqlConnection;

        public ISQLiteConnection Create(string address)
        {
            return this.sqlConnection;
        }
    }
}