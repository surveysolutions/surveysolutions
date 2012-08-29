// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewSynchronizationProcessCreated.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The new synchronization process created.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Synchronization
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

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
        /// Gets or sets the synck type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        #endregion
    }
}