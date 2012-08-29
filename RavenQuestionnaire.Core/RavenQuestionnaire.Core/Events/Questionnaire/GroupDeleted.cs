// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupDeleted.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The group deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:GroupDeleted")]
    public class GroupDeleted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        #endregion
    }
}