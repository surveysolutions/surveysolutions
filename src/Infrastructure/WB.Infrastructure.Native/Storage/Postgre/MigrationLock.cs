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
            try
            {
                this.db.Open();
                this.tr = db.BeginTransaction();

                var statement = isGlobal
                    ? "select pg_advisory_xact_lock (1818, 20433)"
                    : "select pg_advisory_xact_lock (1919, 20433)";

                this.db.Execute(statement);
            }
            catch
            {
                this.db.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                this.tr.Commit();
            }
            finally
            {
                this.db.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await tr.CommitAsync();
            }
            finally
            {
                await db.DisposeAsync();
            }
        }
    }
}
