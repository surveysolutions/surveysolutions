namespace Main.Core.Events.Synchronization
{
    using System;

    using Main.Core.Documents;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new synchronization process created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewSynchronizationProcessCreated")]
    public class NewSynchronizationProcessCreated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        public Guid ProcessGuid { get; set; }

        /// <summary>
        /// Gets or sets ParentProcessKey.
        /// </summary>
        public Guid? ParentProcessKey { get; set; }

        /// <summary>
        /// Gets or sets the synck type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}