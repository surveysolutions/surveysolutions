﻿using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Utility.OrderStrategy
{
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
        public IEnumerable<T> Reorder<T>(IEnumerable<T> list) where T : IAnswer
        {
            return list.OrderBy(n => n.AnswerText);
        }

        #endregion
    }
}