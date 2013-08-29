namespace Main.Core.Commands.User
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The change user command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "ChangeUser")]
    public class ChangeUserCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserCommand"/> class.
        /// </summary>
        public ChangeUserCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="roles">
        /// The roles.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        public ChangeUserCommand(Guid publicKey, string email, UserRoles[] roles, bool isLocked)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.Email = email;
            this.Roles = roles;
            this.IsLocked = isLocked;
        }

        #endregion

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
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public UserRoles[] Roles { get; set; }

        /// <summary>
        /// Gets or sets the supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion
    }
}