// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDocument.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the UserDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The user document.
    /// </summary>
    public class UserDocument : IView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDocument"/> class.
        /// </summary>
        public UserDocument()
        {
            this.CreationDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Roles = new List<UserRoles>();
            this.Location = new LocationDocument();
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
        /// Gets or sets a value indicating whether is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public LocationDocument Location { get; set; }

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
        public List<UserRoles> Roles { get; set; }

        /// <summary>
        /// Gets or sets the supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        public DateTime LastChangeDate { get; set; }

        /// <summary>
        /// Gets user's light info
        /// </summary>
        /// <returns>
        /// UserLight object
        /// </returns>
        public UserLight GetUseLight()
        {
            return new UserLight(this.PublicKey, this.UserName);
        }

        #endregion
    }
}