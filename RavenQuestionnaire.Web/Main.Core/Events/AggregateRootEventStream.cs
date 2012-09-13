// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregateRootEventStream.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AggregateRootEventStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Eventing;

    using Newtonsoft.Json;

    /// <summary>
    /// The aggregate root event stream.
    /// </summary>
    [Serializable]
    public class AggregateRootEventStream
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEventStream"/> class.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <param name="fromVersion">
        /// The from version.
        /// </param>
        /// <param name="toVersion">
        /// The to version.
        /// </param>
        /// <param name="sourceId">
        /// The source id.
        /// </param>
        [JsonConstructor]
        public AggregateRootEventStream(
            IEnumerable<AggregateRootEvent> events, long fromVersion, long toVersion, Guid sourceId)
        {
            this.Events = events.ToArray();
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.SourceId = sourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEventStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public AggregateRootEventStream(CommittedEventStream stream)
        {
            this.Events = stream.Select(e => new AggregateRootEvent(e)).ToArray();
            this.FromVersion = stream.FromVersion;
            this.ToVersion = stream.ToVersion;
            this.SourceId = stream.SourceId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        public AggregateRootEvent[] Events { get; set; }

        /// <summary>
        /// Gets or sets the from version.
        /// </summary>
        public long FromVersion { get; set; }

        // public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets the source id.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Gets or sets the to version.
        /// </summary>
        public long ToVersion { get; set; }

        #endregion

        // public long CurrentSourceVersion { get; set; }
    }
}