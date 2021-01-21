using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class MigrationLock : IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection db;
        private readonly NpgsqlTransaction tr;

        public MigrationLock(string connectionString, long id = 1818)
        {
            this.db = new NpgsqlConnection(connectionString);
            this.db.Open();
            this.tr = db.BeginTransaction();
            this.db.Execute(@$"select pg_advisory_xact_lock ({id}, 20433)");
        }

        public void Dispose()
        {
            this.tr.Commit();
            this.db.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await tr.CommitAsync();
            await db.DisposeAsync();
        }
    }
}
