// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserChanged.cs" company="">
//   
// </copyright>
// <summary>
//   The user changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.User
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The user changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserChanged")]
    public class UserChanged
    {
        /* [AggregateRootId]
        public Guid PublicKey { get; set; }*/
        #region Public Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        //// Is not propogated now
        public UserRoles[] Roles { get; set; }

        #endregion
    }
}