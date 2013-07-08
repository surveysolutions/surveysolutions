using System;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user view input model.
    /// </summary>
    public class UserViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public UserViewInputModel(Guid id)
        {
            this.UserId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewInputModel"/> class.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public UserViewInputModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName { get; private set; }

        #endregion
    }
}