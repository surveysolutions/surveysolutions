using System;
using System.Collections.Generic;
using System.Linq;
using Sqo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Infrastructure.Storage.Mobile.Siaqodb
{
    public class SiaqodbQueryablePlainStorageAccessor<TEntity> : SiaqodbPlainStorageAccessor<TEntity>, IQueryablePlainStorageAccessor<TEntity> where TEntity : class, IPlainStorageEntity
    {
        public SiaqodbQueryablePlainStorageAccessor(ISiaqodb storage) : base(storage)
        {
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            throw new NotSupportedException();

        }

        public IEnumerable<TEntity> Query(Func<TEntity, bool> query)
        {
            return this.Storage.Cast<TEntity>().Where(query).Select(entity => entity);
        }
    }
}