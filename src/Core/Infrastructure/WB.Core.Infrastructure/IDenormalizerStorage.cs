using System;

namespace WB.Core.Infrastructure
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