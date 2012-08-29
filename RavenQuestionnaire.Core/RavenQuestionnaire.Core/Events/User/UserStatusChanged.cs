// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserStatusChanged.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the UserStatusChanged type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.User
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The user status changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserStatusChanged")]
    public class UserStatusChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}