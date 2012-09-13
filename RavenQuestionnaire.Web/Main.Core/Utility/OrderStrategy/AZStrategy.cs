﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AZStrategy.cs" company="">
//   
// </copyright>
// <summary>
//   The az strategy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Utility.OrderStrategy
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

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