// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupDeleted.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events.Questionnaire
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
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDeleted"/> class.
        /// </summary>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        public GroupDeleted(Guid groupPublicKey, Guid parentPublicKey)
        {
            this.GroupPublicKey = groupPublicKey;
            this.ParentPublicKey = parentPublicKey;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid ParentPublicKey { get; set; }

        #endregion
    }
}