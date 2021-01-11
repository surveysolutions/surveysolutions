#nullable enable
using System;
using System.Data;
using Humanizer;
using WB.Core.Infrastructure.PlainStorage;
using System.Linq;
using System.Reflection;
using Dapper;
using Npgsql;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public class PostgresKeyValueStorage<TEntity> : IPlainKeyValueStorage<TEntity>
        where TEntity : class 
    {
        private readonly IUnitOfWork unitOfWork;
        protected readonly IEntitySerializer<TEntity> serializer;

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly string TableName;

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly bool AllowReadFallback = false;
        
        public PostgresKeyValueStorage(IUnitOfWork unitOfWork, IEntitySerializer<TEntity> serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;
        }

        static PostgresKeyValueStorage()
        {
            var type = typeof(TEntity);
            TableName = type.GetInterfaces().Contains(typeof(IStorableEntity))
                    ? type.BaseType?.Name.Pluralize() ?? type.UnderlyingSystemType.Name.Pluralize()
                    : type.Name.Pluralize();

            var appSetting = type.GetCustomAttribute<AppSettingAttribute>();

            if (appSetting != null)
            {
                AllowReadFallback = appSetting.FallbackReadFromPrimaryWorkspace;
            }
        }

        private IDbConnection Connection => this.unitOfWork.Session.Connection;

        public virtual TEntity? GetById(string id)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder(Connection.ConnectionString);

            if (AllowReadFallback)
            { 
                // this will allow PG to read from default schema. LOCAL - scope set to transaction
                Connection.Execute($"SET LOCAL search_path = {connectionBuilder.SearchPath},{WorkspaceContext.Default.SchemaName}");
            }

            string queryResult = Connection.QueryFirstOrDefault<string>(
                $"SELECT value FROM {TableName} WHERE id = @id", new { id });

            if (AllowReadFallback)
            {
                Connection.Execute($"SET LOCAL search_path = {connectionBuilder.SearchPath}");
            }

            return queryResult != null ? this.serializer.Deserialize(queryResult) : null;
        }

        public virtual void Remove(string id)
        {
            int queryResult = Connection.Execute($"DELETE FROM {TableName} WHERE id = @id", new { id });

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
                INSERT into {TableName} (id, value) Values (@id, @value::jsonb)
                ON CONFLICT (id) DO UPDATE SET value = @value::jsonb", new { id, value = serializedValue });
        }

        public bool HasNotEmptyValue(string id)
        {
            return Connection.QuerySingle<bool>(
                $"SELECT exists(select 1 from {TableName} where id = @id and value is not null)",
                new { id });
        }

        public virtual string GetReadableStatus() => "Postgres K/V :/";
    }
}
