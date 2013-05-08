using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Main.DenormalizerStorage
{
    public interface IDenormalizerStorage<T>
        where T : class
    {
        int Count();

        T GetByGuid(Guid key);

        void Remove(Guid key);

        void Store(T denormalizer, Guid key);
    }
}