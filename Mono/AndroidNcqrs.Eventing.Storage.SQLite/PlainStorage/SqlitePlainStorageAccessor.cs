using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.Infrastructure.PlainStorage;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public class SqlitePlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        private readonly SqlitePlainStore documentStore;
        
        public SqlitePlainStorageAccessor(SqlitePlainStore documentStore)
        {
            this.documentStore = documentStore;
        }

        private static string EntityName
        {
            get { return typeof(TEntity).Name; }
        }

        public TEntity GetById(string id)
        {
            string sqliteId = ToSqliteId(id);

            PlainStorageRow row = this.documentStore.GetById(sqliteId);

            if (row == null)
                return null;

            TEntity entity = DeserializeEntity(row.SerializedData);

            return entity;
        }

        public void Remove(string id)
        {
            string sqliteId = ToSqliteId(id);

            this.documentStore.Remove(sqliteId);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Store(TEntity entity, string id)
        {
            string sqliteId = ToSqliteId(id);

            var row = new PlainStorageRow
            {
                Id = sqliteId,
                SerializedData = SerializeEntity(entity),
            };

            this.documentStore.Store(row);
        }

        public void Store(IEnumerable<Tuple<TEntity, string>> entities)
        {
            IEnumerable<PlainStorageRow> rows =
                from entity in entities
                select new PlainStorageRow
                {
                    Id = ToSqliteId(entity.Item2),
                    SerializedData = SerializeEntity(entity.Item1),
                };

            this.documentStore.Store(rows);
        }

        private static string SerializeEntity(TEntity entity)
        {
            return JsonConvert.SerializeObject(entity, Formatting.None, GetJsonSerializerSettings());
        }

        private static TEntity DeserializeEntity(string json)
        {
            return JsonConvert.DeserializeObject<TEntity>(json, GetJsonSerializerSettings());
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        private static string ToSqliteId(string id)
        {
            return string.Format("{0}${1}", EntityName, id);
        }
    }
}