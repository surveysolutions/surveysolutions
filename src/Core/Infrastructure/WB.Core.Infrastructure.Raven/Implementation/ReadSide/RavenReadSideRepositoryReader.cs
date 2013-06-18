using System;
using System.Linq;

using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryReader<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IQueryableReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadLayerStatusService readLayerStatusService;

        public RavenReadSideRepositoryReader(DocumentStore ravenStore, IReadLayerStatusService readLayerStatusService)
            : base(ravenStore)
        {
            this.readLayerStatusService = readLayerStatusService;
        }

        public int Count()
        {
            this.ThrowIfRepositoryIsNotAccessible();

            using (IDocumentSession session = this.OpenSession())
            {
                return
                    session
                        .Query<TEntity>()
                        .Count();
            }
        }

        public TEntity GetById(Guid id)
        {
            this.ThrowIfRepositoryIsNotAccessible();

            string ravenId = ToRavenId(id);

            using (IDocumentSession session = this.OpenSession())
            {
                return session.Load<TEntity>(id: ravenId);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            this.ThrowIfRepositoryIsNotAccessible();

            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session.Query<TEntity>());
            }
        }

        private void ThrowIfRepositoryIsNotAccessible()
        {
            if (this.readLayerStatusService.AreViewsBeingRebuiltNow())
                throw new MaintenanceException("Views are currently being rebuilt. Therefore your request cannot be complete now.");
        }
    }
}