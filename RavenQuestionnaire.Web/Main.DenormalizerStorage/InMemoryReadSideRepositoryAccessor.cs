using System;
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
        private readonly Dictionary<Guid, TView> repository;
        private object locker = new object();
        public InMemoryReadSideRepositoryAccessor()
        {
            this.repository = new Dictionary<Guid, TView>();
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
            lock (locker)
            {
                this.repository.Remove(id);
            }
        }

        public void Store(TView view, Guid id)
        {
            if (!this.repository.ContainsKey(id))
            { 
                lock (locker)
                {
                    if (!this.repository.ContainsKey(id))
                    {
                        this.repository.Add(id, view);
                    }
                    else
                    {
                        this.repository[id] = view;
                    }
                }
            }
        }

        public void Clear()
        {
            this.repository.Clear();
        }
    }
}