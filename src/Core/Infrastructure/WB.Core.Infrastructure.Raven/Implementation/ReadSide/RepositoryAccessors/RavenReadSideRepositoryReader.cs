using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryReader<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IQueryableReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStatusService readSideStatusService;

        public RavenReadSideRepositoryReader(DocumentStore ravenStore, IReadSideStatusService readSideStatusService)
            : base(ravenStore)
        {
            this.readSideStatusService = readSideStatusService;
        }

        protected override TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query)
        {
            this.ThrowIfRepositoryIsNotAccessible();

            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session.Query<TEntity>());
            }
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

        private void ThrowIfRepositoryIsNotAccessible()
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
                throw new MaintenanceException("Views are currently being rebuilt. Therefore your request cannot be complete now.");
        }
    }
}