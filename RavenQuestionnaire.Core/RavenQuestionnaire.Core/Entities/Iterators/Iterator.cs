// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Iterator.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The terator interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    using System.Collections.Generic;

    /// <summary>
    /// The terator interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface Iterator<T> : IEnumerable<T>, IEnumerator<T>
    {
        /*   T First { get; }
        T Last { get; }*/
        #region Public Properties

        /// <summary>
        /// Gets the next.
        /// </summary>
        T Next { get; }

        /// <summary>
        /// Gets the previous.
        /// </summary>
        T Previous { get; }

        #endregion

        /* bool IsDone { get; }*/
        #region Public Methods and Operators

        /// <summary>
        /// The set current.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        void SetCurrent(T item);

        #endregion
    }
}