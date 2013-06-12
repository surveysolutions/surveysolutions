using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.DenormalizerStorage;
using WB.Core.Infrastructure;

namespace CAPI.Androids.Core.Model.Tests
{
    public class FilterableDenormalizerStorageStub<T> : IFilterableDenormalizerStorage<T> where T : class, IView 
    {
        private Dictionary<Guid,T> container;

        public FilterableDenormalizerStorageStub(Dictionary<Guid, T> container)
        {
            this.container = container;
        }

        public int Count()
        {
            return container.Count;
        }

        public T GetById(Guid id)
        {
            if (!container.ContainsKey(id))
                return null;
            return container[id];
        }

        public void Remove(Guid id)
        {
            container.Remove(id);
        }

        public void Store(T view, Guid id)
        {
            container[id] = view;
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
        {
            return container.Select(c => c.Value).Where(predicate.Compile());
        }
    }
}