namespace Core.Supervisor.Views.User
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    ///     The user list item.
    /// </summary>
    public class UserListItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        ///     Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///     Gets or sets the roles.
        /// </summary>
        public List<UserRoles> Roles { get; set; }

        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion
    }
}