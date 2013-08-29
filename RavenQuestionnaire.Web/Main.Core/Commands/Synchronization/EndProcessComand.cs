namespace Main.Core.Commands.Synchronization
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The end process comand.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "EndProcess")]
    public class EndProcessComand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EndProcessComand"/> class.
        /// </summary>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public EndProcessComand(Guid processGuid, EventState status, string description)
        {
            this.ProcessGuid = processGuid;
            this.Status = status;
            this.Description = description;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public EventState Status { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}