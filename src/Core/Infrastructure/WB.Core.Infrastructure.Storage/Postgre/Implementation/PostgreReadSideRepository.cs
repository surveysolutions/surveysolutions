using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    // TODO: Think about session management
    public class PostgreReadSideRepository<TEntity> : IReadSideRepositoryWriter<TEntity>,
        IReadSideRepositoryCleaner,
        IQueryableReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ISessionProvider sessionProvider;

        public PostgreReadSideRepository(ISessionProvider sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public int Count()
        {
            return this.sessionProvider.GetSession().QueryOver<TEntity>().RowCount();
        }

        public TEntity GetById(string id)
        {
            return this.sessionProvider.GetSession().Get<TEntity>(id);
        }

        public void Remove(string id)
        {
            var session = this.sessionProvider.GetSession();
            var entity = session.Load<TEntity>(id);
            session.Delete(entity);
        }

        public void Store(TEntity view, string id)
        {
            this.sessionProvider.GetSession().SaveOrUpdate(null, view, id);
        }

        public void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }

        public void Clear()
        {
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.GetSession().Query<TEntity>());
        }

        public IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> condition = null)
        {
            return this.sessionProvider.GetSession().Query<TEntity>().Where(condition).ToList();
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "PostgreSQL :'(";
        }
    }
}