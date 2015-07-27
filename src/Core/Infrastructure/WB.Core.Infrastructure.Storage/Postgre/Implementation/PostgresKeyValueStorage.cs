using System;
using Humanizer;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgreKeyValueStorage<TEntity> : IReadSideKeyValueStorage<TEntity>, IPlainKeyValueStorage<TEntity>, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly PostgreConnectionSettings settings;
        private readonly string tableName = typeof(TEntity).Name.Pluralize();

        public PostgreKeyValueStorage(PostgreConnectionSettings settings)
        {
            this.settings = settings;
            EnshureTableExists();
        }

        public TEntity GetById(string id)
        {
            using (var conn = new NpgsqlConnection(settings.ConnectionString))
            {
                string commandText = string.Format("SELECT value FROM {0} WHERE id = :id", this.tableName);

                var command = conn.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddWithValue("id", id);

                conn.Open();
                var queryResult = (string)command.ExecuteScalar();
                if (queryResult != null)
                {
                    return JsonConvert.DeserializeObject<TEntity>(queryResult, JsonSerializerSettings);
                }
            }

            return null;
        }

        public void Remove(string id)
        {
            using (var conn = new NpgsqlConnection(settings.ConnectionString))
            {
                string commandText = string.Format("DELETE FROM {0} WHERE id = :id", this.tableName);

                var command = conn.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddWithValue("id", id);

                conn.Open();
                int queryResult = command.ExecuteNonQuery();
                if (queryResult > 1)
                {
                    throw new Exception(
                        string.Format("Unexpected row count of deletec records. Expected to delete not more than 1 row, but affected {0} number of rows",
                        queryResult));
                }
            }
        }

        public void Store(TEntity view, string id)
        {
            bool existing;
            string existsSql = string.Format("SELECT 1 FROM {0} WHERE id = :id LIMIT 1", this.tableName);

            using (var connection = new NpgsqlConnection(this.settings.ConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = existsSql;
                command.Parameters.AddWithValue("id", id);

                connection.Open();
                object queryResult = command.ExecuteScalar();

                existing = queryResult != null;
            }

            string upsertCommandText;
            if (existing)
            {
                upsertCommandText = string.Format("UPDATE {0} SET value = :value WHERE id = :id", this.tableName);
            }
            else
            {
                upsertCommandText = string.Format("INSERT INTO {0} VALUES(:id, :value)", this.tableName);
            }

            using (var connection = new NpgsqlConnection(this.settings.ConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = upsertCommandText;
                command.Parameters.AddWithValue("id", id);

                string serializedValue = JsonConvert.SerializeObject(view, Formatting.None, JsonSerializerSettings);
                command.Parameters.AddWithValue("value", serializedValue);

                connection.Open();
                int queryResult = command.ExecuteNonQuery();
                if (queryResult > 1)
                {
                    throw new Exception(string.Format("Unexpected row count of deletec records. Expected to delete not more than 1 row, but affected {0} number of rows", queryResult));
                }
            }
        }

        public void Clear()
        {
            EnshureTableExists();

            using (var conn = new NpgsqlConnection(settings.ConnectionString))
            {
                string commandText = string.Format("DELETE FROM {0}", this.tableName);
                var command = conn.CreateCommand();
                command.CommandText = commandText;
                conn.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "Postgres Key/Value :/";
        }

        private void EnshureTableExists()
        {
            using (var connection = new NpgsqlConnection(this.settings.ConnectionString))
            {
                connection.Open();
                var command = @"CREATE TABLE IF NOT EXISTS " + this.tableName + @" (
    id        varchar(70) PRIMARY KEY,
    value       JSON NOT NULL
)";
                var sqlCommand = new NpgsqlCommand(command, connection);
                sqlCommand.ExecuteNonQuery();
            }
        }

        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }
    }
}