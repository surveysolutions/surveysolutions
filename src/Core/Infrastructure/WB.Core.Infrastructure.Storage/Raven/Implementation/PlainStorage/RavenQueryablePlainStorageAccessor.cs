using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Raven.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.PlainStorage
{
    public class RavenQueryablePlainStorageAccessor<TEntity> : RavenPlainStorageAccessor<TEntity>, IQueryablePlainStorageAccessor<TEntity> where TEntity : class
    {
        public RavenQueryablePlainStorageAccessor(IRavenPlainStorageProvider storageProvider)
            : base(storageProvider) { }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(session.Query<TEntity>().Customize(customization => customization.WaitForNonStaleResultsAsOfLastWrite()));
            }

        }

        public IEnumerable<TEntity> Query(Expression<TEntity> query)
        {
            throw new NotImplementedException();
        }
    }
}