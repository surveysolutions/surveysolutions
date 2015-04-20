using System;
using System.IO;
using Microsoft.Isam.Esent.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Esent.Implementation
{
    internal class EsentKeyValueStorage<TEntity> : IReadSideKeyValueStorage<TEntity>, IPlainKeyValueStorage<TEntity>, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private PersistentDictionary<string, string> storage;
        private readonly string collectionFolder;

        public EsentKeyValueStorage(EsentSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            string collectionName = typeof(TEntity).Name;
            this.collectionFolder = Path.Combine(settings.Folder, collectionName);

            this.storage = new PersistentDictionary<string, string>(collectionFolder);
        }

        public TEntity GetById(string id)
        {
            string value;
            if (this.storage.TryGetValue(id, out value))
            {
                return JsonConvert.DeserializeObject<TEntity>(value, JsonSerializerSettings);
            }

            return null;
        }

        public void Remove(string id)
        {
            this.storage.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
            this.storage[id] = JsonConvert.SerializeObject(view, Formatting.None, JsonSerializerSettings);
        }

        public void Clear()
        {
            this.storage.Dispose();
            PersistentDictionaryFile.DeleteFiles(this.collectionFolder);
            this.storage = new PersistentDictionary<string, string>(collectionFolder);
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "ESENT :)";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.storage != null)
                {
                    this.storage.Dispose();
                    this.storage = null;
                }

                // free native resources if there are any.
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