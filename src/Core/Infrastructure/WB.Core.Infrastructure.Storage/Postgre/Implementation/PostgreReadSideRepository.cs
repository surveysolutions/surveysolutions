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
        private readonly ISessionFactory sessionFactory;

        public PostgreReadSideRepository(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public int Count()
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.QueryOver<TEntity>().RowCount();
            }
        }


        public TEntity GetById(object id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.Get<TEntity>(id);
            }
        }

        public void Remove(string id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                var entity = session.Load<TEntity>(id);
                session.Delete(entity);
            }
        }

        public void Store(TEntity view, string id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(view);
                    transaction.Commit();
                }
            }
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
            using (var session = sessionFactory.OpenSession())
            {
                return query.Invoke(session.Query<TEntity>());
            }
        }

        public IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> condition = null)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.Query<TEntity>().Where(condition).ToList();
            }
        }
    }
}