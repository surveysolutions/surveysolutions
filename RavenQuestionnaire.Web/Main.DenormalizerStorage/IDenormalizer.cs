// -----------------------------------------------------------------------
// <copyright file="IDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Ninject;

namespace Main.DenormalizerStorage
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDenormalizer
    {
        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        int Count<T>() where T : class;

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        T GetByGuid<T>(Guid key) where T : class;

        /// <summary>
        /// The query.
        /// </summary>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; T].
        /// </returns>
        IQueryable<T> Query<T>() where T : class;

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        void Remove<T>(Guid key) where T : class;

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        void Store<T>(T denormalizer, Guid key) where T : class;
    }

    public class DenormalizerFactory : IDenormalizer
    {
        #region Fields

        /// <summary>
        /// The container.
        /// </summary>
        private readonly IKernel container;

        #endregion

          #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DenormalizerFactory"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public DenormalizerFactory(IKernel container)
        {
            this.container = container;
        }

        #endregion

        protected IDenormalizerStorage<T> GetDenormalizer<T>() where T : class
        {

          
            return this.container.Get<IDenormalizerStorage<T>>();
        }

        #region Implementation of IDenormalizer

        public int Count<T>() where T : class
        {
            return this.GetDenormalizer<T>().Count();
        }

        public T GetByGuid<T>(Guid key) where T : class
        {
            return this.GetDenormalizer<T>().GetByGuid(key);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.GetDenormalizer<T>().Query();
        }

        public void Remove<T>(Guid key) where T : class
        {
            this.GetDenormalizer<T>().Remove(key);
        }

        public void Store<T>(T denormalizer, Guid key) where T : class
        {
            this.GetDenormalizer<T>().Store(denormalizer, key);
        }

        #endregion
    }
}
