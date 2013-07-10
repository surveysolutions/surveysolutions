using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;

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

        public int Count(Expression<Func<TEntity, bool>> query)
        {
            this.ThrowIfRepositoryIsNotAccessible();

            using (IDocumentSession session = this.OpenSession())
            {
                return
                    session
                        .Query<TEntity>()
                        .Count(query);
            }
        }

        public IEnumerable<TEntity> QueryEnumerable(Expression<Func<TEntity, bool>> query)
        {
            this.ThrowIfRepositoryIsNotAccessible();

            var retval = new List<TEntity>();
            foreach (var docBulk in GetAllDocuments(query))
            {
                retval.AddRange(docBulk);

            }
            return retval;
        }

        public IEnumerable<TEntity> QueryEnumerable(Expression<Func<TEntity, bool>> query, int start, int pageSize)
        {
            return GetPagedDocuments(query, start, pageSize).ToList();
        }

        private void ThrowIfRepositoryIsNotAccessible()
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
                throw new MaintenanceException("Views are currently being rebuilt. Therefore your request cannot be complete now.");
        }


        protected IEnumerable<IQueryable<TEntity>> GetAllDocuments(Expression<Func<TEntity, bool>> whereClause)
        {
            const int elementTakeCount = 1024;
            int skipResults = 0;

            while (true)
            {
                var nextGroupOfPoints = GetPagedDocuments(whereClause, skipResults, elementTakeCount);
                if (!nextGroupOfPoints.Any())
                    yield break;
                skipResults += elementTakeCount;
                yield return nextGroupOfPoints;
            }
        }

        protected IQueryable<TEntity> GetPagedDocuments(Expression<Func<TEntity, bool>> whereClause, int start, int pageSize)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return session.Query<TEntity>()
                              .Where(whereClause)
                              .Skip(start)
                              .Take(pageSize);
            }
        }
    }
}