using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public int Count(Expression<Func<TView, bool>> query)
        {
            return
           repository.Values.Where(query.Compile()).Count();
        }

        public IEnumerable<TView> QueryAll(Expression<Func<TView, bool>> query)
        {
            return 
            repository.Values.Where(query.Compile());
        }

        public IQueryable<TView> QueryEnumerable(Expression<Func<TView, bool>> query)
        {
            return
           repository.Values.Where(query.Compile()).AsQueryable();
        }

        public IQueryable<TResult> QueryWithIndex<TResult>(Type index)
        {
            throw new NotImplementedException();
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

        public void Clear()
        {
            this.repository.Clear();
        }
    }
}