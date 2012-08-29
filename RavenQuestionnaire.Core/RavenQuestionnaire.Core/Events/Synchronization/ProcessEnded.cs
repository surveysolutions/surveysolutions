// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessEnded.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the ProcessEnded type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.Synchronization
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

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

        #endregion
    }
}