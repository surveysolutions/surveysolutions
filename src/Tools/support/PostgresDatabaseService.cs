using System.Threading.Tasks;
using Npgsql;

namespace support
{
    public class PostgresDatabaseService : IDatabaseService
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

        public async Task<bool> UpdatePasswordAsync(string connectionString, string login, string passwordHash)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var updateCommand = new NpgsqlCommand($@"UPDATE users.users SET ""PasswordHash""='{passwordHash}' WHERE ""UserName"" = '{login}';", connection);

                var affectedRows = updateCommand.ExecuteNonQuery();

                var selectCommand = new NpgsqlCommand($@"SELECT ""PasswordHash"" FROM users.users WHERE  ""UserName"" = '{login}'", connection);

                var updatedPasswordHash = (string)await selectCommand.ExecuteScalarAsync();

                return updatedPasswordHash == passwordHash;
            }
        }
    }
}
