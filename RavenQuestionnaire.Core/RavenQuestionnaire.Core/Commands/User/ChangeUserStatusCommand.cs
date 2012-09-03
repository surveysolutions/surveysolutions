// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeUserStatusCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The change user status command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.User
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The change user status command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "SetUserLockState")]
    public class ChangeUserStatusCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserStatusCommand"/> class.
        /// </summary>
        public ChangeUserStatusCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserStatusCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        public ChangeUserStatusCommand(Guid publicKey, bool isUserLocked)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.IsUserLocked = isUserLocked;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsUserLocked { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        #endregion
    }
}