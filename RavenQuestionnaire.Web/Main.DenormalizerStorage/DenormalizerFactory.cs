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

    /// <summary>
    /// The denormalizer factory.
    /// </summary>
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

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count<T>() where T : class
        {
            return this.GetDenormalizer<T>().Count();
        }

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public T GetByGuid<T>(Guid key) where T : class
        {
            return this.GetDenormalizer<T>().GetById(key);
        }

        /// <summary>
        /// The query.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        public IQueryable<T> Query<T>() where T : class
        {
            return this.GetDenormalizer<T>().Query();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Remove<T>(Guid key) where T : class
        {
            this.GetDenormalizer<T>().Remove(key);
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Store<T>(T denormalizer, Guid key) where T : class
        {
            this.GetDenormalizer<T>().Store(denormalizer, key);
        }

        #endregion

        #region Methods

        protected IQueryableDenormalizerStorage<T> GetDenormalizer<T>() where T : class
        {
            return this.container.Get<IQueryableDenormalizerStorage<T>>();
        }

        #endregion
    }
}