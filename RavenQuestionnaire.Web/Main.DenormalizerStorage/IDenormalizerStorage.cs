using System;
using System.Linq;

namespace Main.DenormalizerStorage
{
    public interface IDenormalizerStorage<TView>
        where TView : class
    {
        int Count();

        TView GetById(Guid id);

        IQueryable<TView> Query();

        void Remove(Guid id);

        void Store(TView view, Guid id);
    }
}