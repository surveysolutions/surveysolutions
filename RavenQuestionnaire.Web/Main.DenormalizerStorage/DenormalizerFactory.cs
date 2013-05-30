// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DenormalizerFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The denormalizer factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    using Ninject;

    public class DenormalizerFactory : IDenormalizer
    {
        private readonly IKernel container;

        public DenormalizerFactory(IKernel container)
        {
            this.container = container;
        }

        public int Count<T>() where T : class
        {
            return this.GetDenormalizer<T>().Count();
        }

        public T GetByGuid<T>(Guid key) where T : class
        {
            return this.GetDenormalizer<T>().GetById(key);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.GetDenormalizer<T>().Query();
        }

        public TResult Query<T, TResult>(Func<IQueryable<T>, TResult> query) where T : class
        {
            return this.GetDenormalizer<T>().Query(query);
        }

        public void Remove<T>(Guid key) where T : class
        {
            this.GetDenormalizer<T>().Remove(key);
        }

        public void Store<T>(T denormalizer, Guid key) where T : class
        {
            this.GetDenormalizer<T>().Store(denormalizer, key);
        }

        protected IQueryableDenormalizerStorage<T> GetDenormalizer<T>() where T : class
        {
            return this.container.Get<IQueryableDenormalizerStorage<T>>();
        }
    }
}