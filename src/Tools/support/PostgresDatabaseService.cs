using System.Threading.Tasks;
using Npgsql;

namespace support
{
    public class PostgresDatabaseService : IDatabaseSevice
    {
        public async Task<bool> HasConnectionAsync(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                return connection.State == System.Data.ConnectionState.Open;
            }
        }

        public async Task<bool> HasPermissionsAsync(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                        "SELECT pg_catalog.pg_get_userbyid(d.datdba) as \"Owner\" " +
                        "FROM pg_catalog.pg_database d " +
                        $"WHERE d.datname = '{connection.Database}' " +
                        "ORDER BY 1; ", connection);

                var dbOwnerName = (string)await command.ExecuteScalarAsync();

                return dbOwnerName == connection.UserName;
            }
        }
    }
}