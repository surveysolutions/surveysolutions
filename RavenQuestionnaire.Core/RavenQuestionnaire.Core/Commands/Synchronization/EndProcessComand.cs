// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndProcessComand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The end process comand.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Domain;

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
        public EndProcessComand(Guid processGuid, EventState status)
        {
            this.ProcessGuid = processGuid;
            this.Status = status;
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

        #endregion
    }
}