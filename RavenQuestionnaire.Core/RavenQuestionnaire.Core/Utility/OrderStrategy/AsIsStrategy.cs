// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsIsStrategy.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The as is strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;

    /// <summary>
    /// The as is strategy.
    /// </summary>
    public class AsIsStrategy : IOrderStrategy
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
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list)
        {
            return list;
        }

        #endregion
    }
}