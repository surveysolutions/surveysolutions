using System;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
#warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>,
        IReadSideRepositoryWriter<TEntity>, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        public RavenReadSideRepositoryWriter(IDocumentStore ravenStore)
            : base(ravenStore)
        {
        }

        public TEntity GetById(string id)
        {
            return this.GetByIdAvoidingCache(id);
        }

        public void Remove(string id)
        {
            this.RemoveAvoidingCache(id);
        }

        public void Store(TEntity view, string id)
        {
            this.StoreAvoidingCache(view, id);
        }

        private TEntity GetByIdAvoidingCache(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var result = session.Load<TEntity>(id: ravenId);

                return result;
            }
        }

        private void RemoveAvoidingCache(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var view = session.Load<TEntity>(id: ravenId);

                if (view == null)
                    return;

                session.Delete(view);
                session.SaveChanges();
            }
        }

        private void StoreAvoidingCache(TEntity entity, string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                session.Store(entity: entity, id: ravenId);
                session.SaveChanges();
            }
        }

        public void Clear()
        {
            const string DefaultIndexName = "Raven/DocumentsByEntityName";
            if (this.RavenStore.DatabaseCommands.GetIndex(DefaultIndexName) != null)
                this.RavenStore.DatabaseCommands.DeleteByIndex(DefaultIndexName, new IndexQuery()
                {
                    Query = string.Format("Tag: *{0}*", global::Raven.Client.Util.Inflector.Pluralize(ViewName))
                }, new BulkOperationOptions {AllowStale = false}).WaitForCompletion();
        }

        protected override TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}