using System;
using System.Threading.Tasks;
using NLog;
using Npgsql;

namespace support
{
    public class PostgresDatabaseService : IDatabaseService
    {
        private readonly ILogger logger;

        public PostgresDatabaseService(ILogger logger)
        {
            this.logger = logger;
        }

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
            var selectStatement = $@"SELECT ""PasswordHash"" FROM users.users WHERE  ""UserName"" = '{login}'";
            var updateStatement = $@"UPDATE users.users SET ""PasswordHash""='{passwordHash}' WHERE ""UserName"" = '{login}';";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                }catch (Exception ex)
                {
                    logger.Error(ex, "Unable to open connection to DB");
                    throw;
                }

                try
                {
                    var updateCommand = new NpgsqlCommand(updateStatement, connection);
                    var affectedRows = updateCommand.ExecuteNonQuery();
                    logger.Info($"Affected {affectedRows} rows with update statement: {updateStatement}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error during update statement execution: {updateStatement}");
                    throw;
                }

                string updatedPasswordHash;
                try
                {
                    var selectCommand = new NpgsqlCommand(selectStatement, connection);
                    updatedPasswordHash = (string) await selectCommand.ExecuteScalarAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error during update statement execution: {updateStatement}");
                    throw;
                }

                return updatedPasswordHash == passwordHash;
            }
        }
    }
}
