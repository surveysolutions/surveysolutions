// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushEventsCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The push events command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Events;

    /// <summary>
    /// The push events command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "PushAggregateRootEventStream")]
    public class PushEventsCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PushEventsCommand"/> class.
        /// </summary>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="eventChuncks">
        /// The event chuncks.
        /// </param>
        public PushEventsCommand(Guid processGuid, IList<ProcessedEventChunk> eventChuncks)
        {
            this.EventChuncks = eventChuncks;
            this.ProcessGuid = processGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PushEventsCommand"/> class.
        /// </summary>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="eventChuncks">
        /// The event chuncks.
        /// </param>
        public PushEventsCommand(Guid processGuid, IList<IEnumerable<AggregateRootEvent>> eventChuncks)
        {
            // this.AggregateRoots = new[] {new ProcessedEventChunk(aggregateRoots)};
            var result = new List<ProcessedEventChunk>(eventChuncks.Count);
            result.AddRange(eventChuncks.Select(aggregateRootEvents => new ProcessedEventChunk(aggregateRootEvents)));
            this.EventChuncks = result;
            this.ProcessGuid = processGuid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the event chuncks.
        /// </summary>
        public IList<ProcessedEventChunk> EventChuncks { get; set; }

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }

        #endregion
    }
}