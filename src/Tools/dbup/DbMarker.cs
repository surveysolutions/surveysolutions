using System;
using System.Threading.Tasks;
using Npgsql;

namespace dbup
{
    public static class DbMarker
    {
        public static async Task MarkAsZeroMigrationDone(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS ""VersionInfo""
(
  ""Version"" bigint NOT NULL,
  ""AppliedOn"" timestamp without time zone,
  ""Description"" character varying(1024)
)";
                await command.ExecuteNonQueryAsync();
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"INSERT INTO ""VersionInfo"" VALUES(:version, :timeStamp, :desc);";
                insertCommand.Parameters.AddWithValue("version", 1);
                insertCommand.Parameters.AddWithValue("timeStamp", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("desc", "marked as 0 state for db");
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
    }
}