using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Ninject;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgreReadSideRepositoryWithCache<TEntity> : PostgreReadSideRepository<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private MemoryCache cache = new MemoryCache("cache");

        public PostgreReadSideRepositoryWithCache([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public override TEntity GetById(string id)
        {
            if (cache.Contains(id))
                return (TEntity)cache[id];

            var questionare = base.GetById(id);
            cache[id] = questionare;
            return questionare;
        }

        public override void Remove(string id)
        {
            this.cache.Remove(id);
            base.Remove(id);
        }

        public override void Store(TEntity entity, string id)
        {
            this.cache.Remove(id);
            base.Store(entity, id);
        }

        public override void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                string id = tuple.Item2;
                this.cache.Remove(id);
            }

            base.BulkStore(bulk);
        }

        public override void Clear()
        {
            this.cache.Trim(100);
            base.Clear();
        }
    }
}