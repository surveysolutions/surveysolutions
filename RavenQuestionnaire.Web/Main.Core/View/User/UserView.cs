using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user view.
    /// </summary>
    public class UserView
    {
        #region Fields

        /// <summary>
        /// The _primary role.
        /// </summary>
        private UserRoles? primaryRole;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserView"/> class.
        /// </summary>
        public UserView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserView"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="userName">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="creationDate">
        /// The creation date.
        /// </param>
        /// <param name="roles">
        /// The roles.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        /// <param name="supervisor">
        /// The supervisor.
        /// </param>
        /// <param name="locationId">
        /// The location id.
        /// </param>
        public UserView(Guid publicKey, string userName, string password, string email, DateTime creationDate, 
            IEnumerable<UserRoles> roles, bool isLocked, UserLight supervisor, Guid locationId)
        {
            this.PublicKey = publicKey;
            this.UserName = userName;
            this.Password = password;
            this.Email = email;
            this.CreationDate = creationDate;
            this.Roles = roles;
            this.IsLocked = isLocked;
            this.Supervisor = supervisor;
            this.LocationId = locationId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        public Guid LocationId { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the primary role.
        /// </summary>
        public UserRoles PrimaryRole
        {
            get
            {
                if (this.primaryRole.HasValue) 
                    return this.primaryRole.Value;
                if (this.Roles != null) 
                    return this.Roles.FirstOrDefault();
                return UserRoles.User;
            }

            set
            {
                this.primaryRole = value;
            }
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public IEnumerable<UserRoles> Roles { get; set; }

        /// <summary>
        /// Gets or sets the supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The new.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.User.UserView.
        /// </returns>
        public static UserView New()
        {
            return new UserView(
                Guid.Empty, null, null, null, DateTime.UtcNow, new[] { UserRoles.User }, false, null, Guid.Empty);
        }

        #endregion
    }
}