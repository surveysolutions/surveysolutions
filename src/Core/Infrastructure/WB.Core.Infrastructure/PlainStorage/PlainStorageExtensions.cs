using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.PlainStorage
{
    public static class PlainStorageExtensions
    {
        public static PlainStorageVersionedWrapper<TEntity> AsVersioned<TEntity>(this IPlainStorageAccessor<TEntity> storage) where TEntity : class
        {
            return new PlainStorageVersionedWrapper<TEntity>(storage);
        }
    }
}
