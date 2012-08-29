// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregateRootEventStreamPushed.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The aggregate root event stream pushed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Synchronization
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The aggregate root event stream pushed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AggregateRootEventStreamPushed")]
    public class AggregateRootEventStreamPushed
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the aggregate roots.
        /// </summary>
        public IEnumerable<ProcessedEventChunk> AggregateRoots { get; set; }

        #endregion
    }
}