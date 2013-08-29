namespace Main.Core.Events.Synchronization
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;

    using Ncqrs.Eventing.Storage;

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