// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KernelLocator.cs" company="">
//   
// </copyright>
// <summary>
//   The kernel locator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using Ninject;

    /// <summary>
    /// The kernel locator.
    /// </summary>
    public static class KernelLocator
    {
        #region Static Fields

        /// <summary>
        /// The _kernel.
        /// </summary>
        private static IKernel _kernel;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the kernel.
        /// </summary>
        public static IKernel Kernel
        {
            get
            {
                return _kernel;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The set kernel.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public static void SetKernel(IKernel kernel)
        {
            _kernel = kernel;
        }

        #endregion
    }
}