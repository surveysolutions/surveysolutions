// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserViewInputModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.User
{
    using System;

    /// <summary>
    /// The user view input model.
    /// </summary>
    public class UserViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The provider user key.
        /// </param>
        public UserViewInputModel(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewInputModel"/> class.
        /// </summary>
        /// <param name="UserName">
        /// The User name.
        /// </param>
        /// <param name="UserEmail">
        /// The User email.
        /// </param>
        public UserViewInputModel(string UserName, string UserEmail)
        {
            this.UserName = UserName;
            this.UserEmail = UserEmail;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     User Id
        /// </summary>
        public Guid? PublicKey { get; protected set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserEmail { get; protected set; }

        /// <summary>
        ///     User email
        /// </summary>
        public string UserName { get; protected set; }

        #endregion
    }
}