// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewRepository.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The view repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ninject;

namespace Main.Core.View
{
    /// <summary>
    /// The view repository.
    /// </summary>
    public class ViewRepository : IViewRepository
    {
        #region Fields

        /// <summary>
        /// The container.
        /// </summary>
        private readonly IKernel container;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRepository"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public ViewRepository(IKernel container)
        {
            this.container = container;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <typeparam name="TInput">
        /// </typeparam>
        /// <typeparam name="TOutput">
        /// </typeparam>
        /// <returns>
        /// The TOutput.
        /// </returns>
        public TOutput Load<TInput, TOutput>(TInput input)
        {
            var factory = this.container.TryGet<IViewFactory<TInput, TOutput>>();
            if (factory == null)
            {
                return default(TOutput);
            }

            return factory.Load(input);
        }

        #endregion
    }
}