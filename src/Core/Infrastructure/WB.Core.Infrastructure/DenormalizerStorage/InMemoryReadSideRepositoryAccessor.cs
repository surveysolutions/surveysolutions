using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.DenormalizerStorage
{
    public class InMemoryReadSideRepositoryAccessor<TView, TKey> : IQueryableReadSideRepositoryReader<TView, TKey>,
        IReadSideRepositoryWriter<TView, TKey>,
        IReadSideKeyValueStorage<TView, TKey>
        where TView : class, IReadSideRepositoryEntity
    {
        private readonly Dictionary<TKey, TView> repository;
        private object locker = new object();
        public InMemoryReadSideRepositoryAccessor()
        {
            this.repository = new Dictionary<TKey, TView>();
        }

        public InMemoryReadSideRepositoryAccessor(Dictionary<TKey, TView> entities)
        {
            this.repository = entities ?? new Dictionary<TKey, TView>();
        }

        public int Count()
        {
            return this.repository.Count;
        }

        public TView GetById(TKey id)
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

        public void Remove(TKey id)
        {
            lock (locker)
            {
                this.repository.Remove(id);
            }
        }

        public void Store(TView view, TKey id)
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

        public void BulkStore(List<Tuple<TView, TKey>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }

        public void Flush()
        {

        }

        public void Clear()
        {
            this.repository.Clear();
        }
    }


    public class InMemoryReadSideRepositoryAccessor<TView> : InMemoryReadSideRepositoryAccessor<TView, string>,
        IQueryableReadSideRepositoryReader<TView>,
        IReadSideRepositoryWriter<TView>,
        IReadSideKeyValueStorage<TView>
        where TView : class, IReadSideRepositoryEntity
    {
    }
}
