using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class SiaqoDbAccessor<TEntity> : SiaqDbRepository<TEntity>, IQueryablePlainStorageAccessor<TEntity> where TEntity : Entity
    {
        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            throw new NotSupportedException();

        }

        public IEnumerable<TEntity> Query(Func<TEntity, bool> query)
        {
            return SiaqodbFactory.GetInstance().Cast<TEntity>().Where(query).Select(entity => entity);
        }
    }
}