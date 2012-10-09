// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventSyncExtensions.cs" company="World bank">
//   2012 
// </copyright>
// <summary>
//   The event sync extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System.Collections.Generic;

    using Main.Core.Utility;

    /// <summary>
    /// The event sync extensions.
    /// </summary>
    public static class EventSyncExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read events by chunks.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunksize">
        /// The chunksize.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent]].
        /// </returns>
        public static IEnumerable<IEnumerable<AggregateRootEvent>> ReadEventsByChunks(
            this IEventSync source, int chunksize = 2048)
        {
            IEnumerable<AggregateRootEvent> events = source.ReadEvents();

            return events.Chunk(chunksize, (e, previous) => e.CommitId == previous.CommitId);
        }

        #endregion
    }
}