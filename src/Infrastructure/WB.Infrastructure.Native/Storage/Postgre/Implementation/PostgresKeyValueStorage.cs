using System;
using System.Collections.Concurrent;
using System.Data.Common;
using Humanizer;
using Npgsql;
using NpgsqlTypes;
using WB.Core.Infrastructure.PlainStorage;
using System.Linq;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorage<TEntity>
        where TEntity : class
    {
        protected readonly string tableName;
        private readonly IUnitOfWork unitOfWork;
        protected readonly IEntitySerializer<TEntity> serializer;

        private static readonly ConcurrentDictionary<Type, string> _tableNamesMap = new ConcurrentDictionary<Type, string>();

        public PostgresKeyValueStorage(IUnitOfWork unitOfWork, IEntitySerializer<TEntity> serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;

            tableName = _tableNamesMap.GetOrAdd(typeof(TEntity),
                (type) => type.GetInterfaces().Contains(typeof(IStorableEntity))
                    ? type.BaseType.Name.Pluralize()
                    : type.Name.Pluralize());
        }

        public virtual TEntity GetById(string id)
        {
            EnsureTableExists();

            string queryResult;
            using (var command = new NpgsqlCommand())
            {
                string commandText = $"SELECT value FROM {this.tableName} WHERE id = :id";

                command.CommandText = commandText;
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = id};
                command.Parameters.Add(parameter);

                queryResult = (string) this.ExecuteScalar(command);
            }

            if (queryResult != null)
            {
                return this.serializer.Deserialize(queryResult);
            }

            return null;
        }

        protected abstract object ExecuteScalar(DbCommand command);
        protected abstract int ExecuteNonQuery(DbCommand command);

        public virtual void Remove(string id)
        {
            EnsureTableExists();

            int queryResult;
            using (var command = new NpgsqlCommand())
            {
                command.CommandText = $"DELETE FROM {this.tableName} WHERE id = :id";
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = id};
                command.Parameters.Add(parameter);

                queryResult = this.ExecuteNonQuery(command);
            }

            if (queryResult > 1)
            {
                throw new Exception(
                    $"Unexpected row count of deleted records. Expected to delete 1 row, but affected {queryResult} number of rows");
            }
        }

        public virtual void Store(TEntity view, string id)
        {
            EnsureTableExists();

            bool entityExists;
            using (var existsCommand = new NpgsqlCommand())
            {
                existsCommand.CommandText = $"SELECT 1 FROM {this.tableName} WHERE id = :id LIMIT 1";

                var idParameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = id};

                existsCommand.Parameters.Add(idParameter);

                object existsResult = this.ExecuteScalar(existsCommand);

                entityExists = existsResult != null;
            }

            using (var upsertCommand = new NpgsqlCommand())
            {
                upsertCommand.CommandText = entityExists
                    ? $"UPDATE {this.tableName} SET value = :value WHERE id = :id"
                    : $"INSERT INTO {this.tableName} VALUES(:id, :value)";

                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = id};
                var serializedValue = this.serializer.Serialize(view);
                var valueParameter = new NpgsqlParameter("value", NpgsqlDbType.Jsonb) {Value = serializedValue};

                upsertCommand.Parameters.Add(parameter);
                upsertCommand.Parameters.Add(valueParameter);

                var queryResult = this.ExecuteNonQuery(upsertCommand);

                if (queryResult > 1)
                {
                    throw new Exception(
                        $"Unexpected row count of deleted records. Expected to delete not more than 1 row, but affected {queryResult} number of rows");
                }
            }
        }

        public bool HasNotEmptyValue(string id)
        {
            EnsureTableExists();

            using (var existsCommand = new NpgsqlCommand())
            {
                existsCommand.CommandText = $"SELECT 1 FROM {this.tableName} WHERE id = :id AND value IS NOT NULL";
                var idParameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = id};
                existsCommand.Parameters.Add(idParameter);

                object existsResult = this.ExecuteScalar(existsCommand);

                return existsResult != null;
            }
        }

        public void Dispose()
        {
        }

        public Type ViewType => typeof(TEntity);

        public virtual string GetReadableStatus() => "Postgres K/V :/";

        private static bool doesExistTable = false;

        protected void EnsureTableExists()
        {
            if (doesExistTable) return;

            var command = $@"CREATE TABLE IF NOT EXISTS {this.tableName} (
    id        text PRIMARY KEY,
    value       JSON NOT NULL
)";
            using (var sqlCommand = this.unitOfWork.Session.Connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }

            doesExistTable = true;
        }
    }
}
