using System;

using Raven.Client;
using Raven.Client.Document;

using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IReadSideRepositoryWriter<TEntity>, IRavenReadSideRepositoryWriter
        where TEntity : class, IReadSideRepositoryEntity
    {
        internal RavenReadSideRepositoryWriter(DocumentStore ravenStore, IRavenReadSideRepositoryWriterRegistry writerRegistry)
            : base(ravenStore)
        {
            writerRegistry.Register(this);
        }

        public TEntity GetById(Guid id)
        {
            string ravenId = ToRavenId(id);

            using (IDocumentSession session = this.OpenSession())
            {
                return session.Load<TEntity>(id: ravenId);
            }
        }

        public void Remove(Guid id)
        {
            string ravenId = ToRavenId(id);

            using (IDocumentSession session = this.OpenSession())
            {
                var view = session.Load<TEntity>(id: ravenId);

                session.Delete(view);
                session.SaveChanges();
            }
        }

        public void Store(TEntity view, Guid id)
        {
            string ravenId = ToRavenId(id);

            using (IDocumentSession session = this.OpenSession())
            {
                session.Store(entity: view, id: ravenId);
                session.SaveChanges();
            }
        }
    }
}