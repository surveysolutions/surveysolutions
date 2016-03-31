﻿using System;
using System.Collections.Generic;
using System.Data;
using Humanizer;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable.Services;
using Newtonsoft.Json;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorage<TEntity> 
        where TEntity: class
    {
        protected readonly string connectionString;
        protected readonly string tableName = typeof(TEntity).Name.Pluralize();
        private readonly ILogger logger;

        public PostgresKeyValueStorage(string connectionString, ILogger logger)
        {
            this.connectionString = connectionString;
            this.logger = logger;
            this.EnshureTableExists();
        }

        public virtual TEntity GetById(string id)
        {
            string queryResult;
            using (var command = new NpgsqlCommand())
            {
                string commandText = $"SELECT value FROM {this.tableName} WHERE id = :id";

                command.CommandText = commandText;
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                command.Parameters.Add(parameter);

                queryResult = (string) this.ExecuteScalar(command);
            }

            if (queryResult != null)
            {
                return JsonConvert.DeserializeObject<TEntity>(queryResult, BackwardCompatibleJsonSerializerSettings);
            }

            return null;
        }

        protected abstract object ExecuteScalar(IDbCommand command);
        protected abstract int ExecuteNonQuery(IDbCommand command);
        
        public virtual void Remove(string id)
        {
            int queryResult;
            using (var command = new NpgsqlCommand())
            {
                command.CommandText = $"DELETE FROM {this.tableName} WHERE id = :id";
                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
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
            bool entityExists;
            using (var existsCommand = new NpgsqlCommand())
            {
                existsCommand.CommandText = $"SELECT 1 FROM {this.tableName} WHERE id = :id LIMIT 1";

                var idParameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };

                existsCommand.Parameters.Add(idParameter);

                object existsResult = this.ExecuteScalar(existsCommand);

                entityExists = existsResult != null;
            }

            using (var upsertCommand = new NpgsqlCommand())
            {
                upsertCommand.CommandText = entityExists
                    ? $"UPDATE {this.tableName} SET value = :value WHERE id = :id"
                    : $"INSERT INTO {this.tableName} VALUES(:id, :value)";

                var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                string serializedValue = JsonConvert.SerializeObject(view, Formatting.None, BackwardCompatibleJsonSerializerSettings);
                var valueParameter = new NpgsqlParameter("value", NpgsqlDbType.Json) { Value = serializedValue };

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

        public virtual void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            try
            {
                this.FastBulkStore(bulk);
            }
            catch (Exception exception)
            {
                this.logger.Warn($"Failed to store bulk of {bulk.Count} entities of type {this.ViewType.Name} using fast way. Switching to slow way.", exception);

                this.SlowBulkStore(bulk);
            }
        }

        public virtual void Clear()
        {
            this.EnshureTableExists();

            using (var command = new NpgsqlCommand())
            {
                command.CommandText = $"DELETE FROM {this.tableName}";
                this.ExecuteNonQuery(command);
            }
        }

        public void Dispose() {}

        public Type ViewType => typeof(TEntity);

        public virtual string GetReadableStatus() => "Postgres K/V :/";

        private void FastBulkStore(List<Tuple<TEntity, string>> bulk)
        {
            this.EnshureTableExists();

            using (var connection = new NpgsqlConnection(this.connectionString))
            {
                connection.Open();
                using (var writer = connection.BeginBinaryImport($"COPY {this.tableName}(id, value) FROM STDIN BINARY;"))
                {
                    foreach (var item in bulk)
                    {
                        writer.StartRow();
                        writer.Write(item.Item2, NpgsqlDbType.Text); // write Id
                        var serializedValue = JsonConvert.SerializeObject(item.Item1, Formatting.None, BackwardCompatibleJsonSerializerSettings);
                        writer.Write(serializedValue, NpgsqlDbType.Json); // write value
                    }
                }
            }
        }

        private void SlowBulkStore(List<Tuple<TEntity, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                var entity = tuple.Item1;
                var id = tuple.Item2;

                this.Store(entity, id);
            }
        }

        protected void EnshureTableExists()
        {
            using (var connection = new NpgsqlConnection(this.connectionString))
            {
                connection.Open();
                var command = @"CREATE TABLE IF NOT EXISTS " + this.tableName + @" (
    id        text PRIMARY KEY,
    value       JSON NOT NULL
)";
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static readonly JsonSerializerSettings BackwardCompatibleJsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Binder = new OldToNewAssemblyRedirectSerializationBinder()
        };
    }
}