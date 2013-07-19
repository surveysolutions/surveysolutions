namespace Main.Core.Events.User
{
    using System;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new user created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserCreated")]
    public class NewUserCreated : UserBaseEvent
    {
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
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public UserRoles[] Roles { get; set; }

        /// <summary>
        /// Gets or sets the supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

        #endregion

        protected override bool DoCheckIsAssignedRole(UserRoles role)
        {
            return Roles != null && Roles.Where(r => r == role).Count() > 0;
        }

    }
}