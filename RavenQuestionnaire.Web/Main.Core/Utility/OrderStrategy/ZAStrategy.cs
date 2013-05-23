// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZAStrategy.cs" company="">
//   
// </copyright>
// <summary>
//   The za strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The za strategy.
    /// </summary>
    public class ZAStrategy : IOrderStrategy
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
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list) where T : IAnswer
        {
            return list.OrderByDescending(n => n.AnswerText);
        }

        #endregion
    }
}