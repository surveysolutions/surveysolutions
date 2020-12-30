using System;
using System.Collections.Concurrent;
using System.Data;
using Humanizer;
using WB.Core.Infrastructure.PlainStorage;
using System.Linq;
using Dapper;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorage<TEntity>
        where TEntity : class
    {
        protected readonly string tableName;
        private readonly IUnitOfWork unitOfWork;
        protected readonly IEntitySerializer<TEntity> serializer;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, string> tableNamesMap = new();

        protected PostgresKeyValueStorage(IUnitOfWork unitOfWork, IEntitySerializer<TEntity> serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;

            tableName = tableNamesMap.GetOrAdd(typeof(TEntity),
                (type) => type.GetInterfaces().Contains(typeof(IStorableEntity))
                    ? type.BaseType.Name.Pluralize()
                    : type.Name.Pluralize());
        }

        private IDbConnection Connection => this.unitOfWork.Session.Connection;

        public virtual TEntity GetById(string id)
        {
            string queryResult = Connection.QueryFirstOrDefault<string>(
                $"SELECT value FROM {this.tableName} WHERE id = @id", new { id });

            if (queryResult != null)
            {
                return this.serializer.Deserialize(queryResult);
            }

            return null;
        }

        public virtual void Remove(string id)
        {
            int queryResult = Connection.Execute($"DELETE FROM {this.tableName} WHERE id = @id", new { id });

            if (queryResult > 1)
            {
                throw new Exception(
                    $"Unexpected row count of deleted records. Expected to delete 1 row, but affected {queryResult} number of rows");
            }
        }

        public virtual void Store(TEntity view, string id)
        {
            var serializedValue = this.serializer.Serialize(view);
            Connection.Execute($@"
                INSERT into {this.tableName} (id, value) Values (@id, @value::jsonb)
                ON CONFLICT (id) DO UPDATE SET value = @value::jsonb", new { id, value = serializedValue });
        }


        public bool HasNotEmptyValue(string id)
        {
            return Connection.QuerySingle<bool>(
                $"SELECT exists(select 1 from {this.tableName} where id = @id and value is not null)", 
                new {id});
        }
        
        public virtual string GetReadableStatus() => "Postgres K/V :/";
    }
}
