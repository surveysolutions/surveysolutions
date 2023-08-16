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

        public MigrationLock(NpgsqlConnectionStringBuilder connectionStringBuilder, bool isGlobal = true)
        {
            this.db = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            this.db.Open();
            this.tr = db.BeginTransaction();
            
            var statement = isGlobal
                ? "select pg_advisory_xact_lock (1818, 20433)"
                : "select pg_advisory_xact_lock (1919, 20433)";
            
            this.db.Execute(statement);
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
