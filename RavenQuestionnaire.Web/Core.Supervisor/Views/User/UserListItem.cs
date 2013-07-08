using System;
using Core.Supervisor.Views.Interviewer;

namespace Core.Supervisor.Views.User
{
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    ///     The user list item.
    /// </summary>
    public class UserListItem : InterviewersItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserListItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="creationDate">
        /// The creation date.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        public UserListItem(Guid id, string name, string email, DateTime creationDate, bool isLocked, List<UserRoles> roles)
            :base(id, name, email, creationDate, isLocked)
        {
            this.Roles = roles;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the roles.
        /// </summary>
        public List<UserRoles> Roles { get; set; }

        #endregion
    }
}