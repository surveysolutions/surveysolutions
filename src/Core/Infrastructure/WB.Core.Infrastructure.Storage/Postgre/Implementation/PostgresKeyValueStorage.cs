using System;
using System.Data;
using Humanizer;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorage<TEntity> where TEntity: class
    {
        private readonly ISessionProvider sessionProvider;
        private readonly string connectionString;
        private readonly string tableName = typeof(TEntity).Name.Pluralize();

        public PostgresKeyValueStorage(ISessionProvider sessionProvider, string connectionString)
        {
            this.sessionProvider = sessionProvider;
            this.connectionString = connectionString;
            EnshureTableExists();
        }

        public TEntity GetById(string id)
        {
            string queryResult;
            using (var command = sessionProvider.GetSession().Connection.CreateCommand())
            {
                string commandText = string.Format("SELECT value FROM {0} WHERE id = :id", this.tableName);

                command.CommandText = commandText;
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                command.Parameters.Add(parameter);

                EnlistIntoCurrentTransaction(command);

                queryResult = (string) command.ExecuteScalar();
            }

            if (queryResult != null)
            {
                return JsonConvert.DeserializeObject<TEntity>(queryResult, JsonSerializerSettings);
            }

            return null;
        }

        public void Remove(string id)
        {
            int queryResult;
            using (var command = sessionProvider.GetSession().Connection.CreateCommand())
            {
                command.CommandText = string.Format("DELETE FROM {0} WHERE id = :id", this.tableName);
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                command.Parameters.Add(parameter);

                EnlistIntoCurrentTransaction(command);
                queryResult = command.ExecuteNonQuery();
            }
            if (queryResult > 1)
            {
                throw new Exception(
                    string.Format(
                        "Unexpected row count of deleted records. Expected to delete 1 row, but affected {0} number of rows",
                        queryResult));
            }
        }

        public void Store(TEntity view, string id)
        {
            object existsResult;
            using (var existsCommand = sessionProvider.GetSession().Connection.CreateCommand())
            {
                existsCommand.CommandText = string.Format("SELECT 1 FROM {0} WHERE id = :id LIMIT 1", this.tableName);

                var idParameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };

                existsCommand.Parameters.Add(idParameter);

                EnlistIntoCurrentTransaction(existsCommand);
                existsResult = existsCommand.ExecuteScalar();
            }

            var existing = existsResult != null;

            string commandText;
            if (existing)
            {
                commandText = string.Format("UPDATE {0} SET value = :value WHERE id = :id", this.tableName);
            }
            else
            {
                commandText = string.Format("INSERT INTO {0} VALUES(:id, :value)", this.tableName);
            }


            int queryResult;
            using (var upsertCommand = sessionProvider.GetSession().Connection.CreateCommand())
            {
                upsertCommand.CommandText = commandText;

                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                string serializedValue = JsonConvert.SerializeObject(view, Formatting.None, JsonSerializerSettings);
                var valueParameter = new NpgsqlParameter("value", NpgsqlDbType.Json) { Value = serializedValue };

                upsertCommand.Parameters.Add(parameter);
                upsertCommand.Parameters.Add(valueParameter);

                EnlistIntoCurrentTransaction(upsertCommand);
                queryResult = upsertCommand.ExecuteNonQuery();
            }
            if (queryResult > 1)
            {
                throw new Exception(string.Format("Unexpected row count of deleted records. Expected to delete not more than 1 row, but affected {0} number of rows", queryResult));
            }
        }

        public void Clear()
        {
            EnshureTableExists();

            using (var command = sessionProvider.GetSession().Connection.CreateCommand())
            {
                command.CommandText = string.Format("DELETE FROM {0}", this.tableName);
                EnlistIntoCurrentTransaction(command);
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
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = @"CREATE TABLE IF NOT EXISTS " + this.tableName + @" (
    id        varchar(70) PRIMARY KEY,
    value       JSON NOT NULL
)";
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
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

        private void EnlistIntoCurrentTransaction(IDbCommand command)
        {
            var activeTransaction = sessionProvider.GetSession().Transaction;
            if (activeTransaction.IsActive)
            {
                activeTransaction.Enlist(command);
            }
        }
    }
}