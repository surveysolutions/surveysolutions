using System;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Synchronization
{
    /// <summary>
    /// The process ended.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:ProcessEnded")]
    public class ProcessEnded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public EventState Status { get; set; }

        /// <summary>
        /// Gets or sets ProcessKey.
        /// </summary>
        public Guid ProcessKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }
        
        #endregion
    }
}