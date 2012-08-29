// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOrderStrategy.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The OrderStrategy interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;

    /// <summary>
    /// The OrderStrategy interface.
    /// </summary>
    public interface IOrderStrategy
    {
        #region Public Methods and Operators

        /// <summary>
        /// The reorder.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        IEnumerable<T> Reorder<T>(IEnumerable<T> list);

        #endregion

        // IEnumerable<T> Reorder<T>(IEnumerable<T> list, Func<T,object> )
    }
}