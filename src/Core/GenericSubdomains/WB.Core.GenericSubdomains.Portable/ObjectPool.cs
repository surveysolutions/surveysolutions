using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable
{
    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;
        private readonly Action<T> _dispose;

        public ObjectPool(Func<T> objectGenerator, Action<T> dispose = null)
        {
            if (objectGenerator == null)
                throw new ArgumentNullException(nameof(objectGenerator));

            this._objects = new ConcurrentBag<T>();
            this._objectGenerator = objectGenerator;
            this._dispose = dispose;

            this.DisposableAction = item =>
            {
                this.PutObject(item);
            };
        }

        public T GetObject()
        {
            T item;
            if (this._objects.TryTake(out item))
                return item;
            return this._objectGenerator();
        }

        public void PutObject(T item)
        {
            this._dispose?.Invoke(item);
            this._objects.Add(item);
        }

        public PoolItem Object
        {
            get
            {
                var res = Disposable.GetObject();
                res.Value = this.GetObject();
                res.DisposeAction = DisposableAction;
                return res;
            }
        }

        private readonly Action<T> DisposableAction;
        
        private static readonly ObjectPool<PoolItem> Disposable = new ObjectPool<PoolItem>(() => new PoolItem());

        public class PoolItem : IDisposable
        {
            public T Value { get; set; }
            internal Action<T> DisposeAction { get; set; }

            public void Dispose()
            {
                this.DisposeAction?.Invoke(this.Value);
                Disposable.PutObject(this);
            }
        }
    }
    
    public static class Pool
    {
        public static ObjectPool<StringBuilder> StringBuilder = new ObjectPool<StringBuilder>(() => new StringBuilder(), sb => sb.Clear());
        
    }

    public static class Pool<T>
    {
        public static ObjectPool<Stack<T>> Stack = new ObjectPool<Stack<T>>(() => new Stack<T>(), st => st.Clear());
        public static ObjectPool<Queue<T>> Queue = new ObjectPool<Queue<T>>(() => new Queue<T>(), st => st.Clear());
    }
}
