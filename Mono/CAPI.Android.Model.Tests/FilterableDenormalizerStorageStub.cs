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
using Ncqrs.Domain;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Androids.Core.Model.Tests
{
    public class FilterableDenormalizerStorageStub<T> : IFilterableReadSideRepositoryWriter<T>
        where T : class, IView 
    {
        private Dictionary<string, T> container;

        public FilterableDenormalizerStorageStub(Dictionary<Guid, T> container)
        {
            this.container = container.ToDictionary(x => x.Key.ToString(), x => x.Value);
        }

        public int Count()
        {
            return container.Count;
        }

        public T GetById(string id)
        {
            if (!container.ContainsKey(id))
                return null;
            return container[id];
        }

        public void Remove(string id)
        {
            container.Remove(id);
        }

        public void Store(T view, string id)
        {
            container[id] = view;
        }

        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            return container.Select(c => c.Value).Where(predicate.Compile());
        }
    }
}