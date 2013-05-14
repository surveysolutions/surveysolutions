using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Main.DenormalizerStorage
{
    public interface IDenormalizerStorage<TView>
        where TView : class
    {
        int Count();

        TView GetById(Guid id);

        void Remove(Guid id);

        void Store(TView view, Guid id);
    }
}