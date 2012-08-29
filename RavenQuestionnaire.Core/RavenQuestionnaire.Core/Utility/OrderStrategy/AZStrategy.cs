// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AZStrategy.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The az strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The az strategy.
    /// </summary>
    public class AZStrategy : IOrderStrategy
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
            return list.OrderBy(n => (n as Answer).AnswerText);
        }

        #endregion
    }
}