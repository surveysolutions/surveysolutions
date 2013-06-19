using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.DenormalizerStorage
{
    public class InMemoryReadSideRepositoryAccessor<TView> : IQueryableReadSideRepositoryReader<TView>, IReadSideRepositoryWriter<TView>
        where TView : class, IView
    {
        private readonly ConcurrentDictionary<Guid, TView> repository;

        public InMemoryReadSideRepositoryAccessor()
        {
            this.repository = new ConcurrentDictionary<Guid, TView>();
        }

        public int Count()
        {
            return this.repository.Count;
        }

        public TView GetById(Guid id)
        {
            if (!this.repository.ContainsKey(id))
            {
                return null;
            }

            return this.repository[id];
        }

        public TResult Query<TResult>(Func<IQueryable<TView>, TResult> query)
        {
            return query.Invoke(this.repository.Values.AsQueryable());
        }

        public void Remove(Guid id)
        {
            TView val;
            this.repository.TryRemove(id, out val);
        }

        public void Store(TView view, Guid id)
        {
            if (this.repository.ContainsKey(id))
            {
                this.repository[id] = view;
                return;
            }

            this.repository.TryAdd(id, view);
        }

        public void Clear()
        {
            this.repository.Clear();
        }
    }
}