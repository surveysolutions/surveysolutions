// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the IEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events
{
    using System.Collections.Generic;

    /// <summary>
    /// The EventSync interface.
    /// </summary>
    public interface IEventStreamReader
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns list of ALL events grouped by aggregate root, please use very carefully
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent].
        /// </returns>
        IEnumerable<AggregateRootEvent> ReadEvents();

        #endregion
    }
}