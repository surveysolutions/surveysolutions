using System;
using System.Linq;

using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IReadSideRepositoryWriter<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        public RavenReadSideRepositoryWriter(DocumentStore ravenStore)
            : base(ravenStore) { }

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