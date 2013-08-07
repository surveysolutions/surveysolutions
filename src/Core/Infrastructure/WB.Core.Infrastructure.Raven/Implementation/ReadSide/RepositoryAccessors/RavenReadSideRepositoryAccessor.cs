using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public abstract class RavenReadSideRepositoryAccessor<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly DocumentStore ravenStore;

        protected RavenReadSideRepositoryAccessor(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        protected abstract TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query);

        private static string ViewName
        {
            get { return typeof(TEntity).FullName; }
        }

        protected IDocumentSession OpenSession()
        {
            this.ravenStore.DatabaseCommands.EnsureDatabaseExists("Views");
            return this.ravenStore.OpenSession("Views");
        }

        protected static string ToRavenId(Guid id)
        {
            return string.Format("{0}${1}", ViewName, id.ToString());
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return this.QueryImpl(query);
        }

        public IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> query)
        {
            var retval = new List<TEntity>();
            foreach (var docBulk in this.GetAllDocuments(query))
            {
                retval.AddRange(docBulk);
            }
            return retval;
        }

        private int MaxNumberOfRequestsPerSession
        {
            get { return this.ravenStore.Conventions.MaxNumberOfRequestsPerSession; }
        }

        private IEnumerable<IQueryable<TEntity>> GetAllDocuments(Expression<Func<TEntity, bool>> whereClause)
        {
            int skipResults = 0;

            while (true)
            {
                var nextGroupOfPoints = this.GetPagedDocuments(whereClause, skipResults, this.MaxNumberOfRequestsPerSession);
                if (!nextGroupOfPoints.Any())
                    yield break;
                skipResults += this.MaxNumberOfRequestsPerSession;
                yield return nextGroupOfPoints;
            }
        }

        private IQueryable<TEntity> GetPagedDocuments(Expression<Func<TEntity, bool>> whereClause, int start, int pageSize)
        {
            return this.QueryImpl(queryable => Queryable.Skip(queryable.Where(whereClause), start).Take(pageSize));
        }
    }
}