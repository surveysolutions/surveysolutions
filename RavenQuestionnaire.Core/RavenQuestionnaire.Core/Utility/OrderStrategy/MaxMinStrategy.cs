// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaxMinStrategy.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The max min strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The max min strategy.
    /// </summary>
    public class MaxMinStrategy : IOrderStrategy
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
            return list.OrderByDescending(n => (n as Answer).AnswerValue).ToList();
        }

        #endregion
    }
}