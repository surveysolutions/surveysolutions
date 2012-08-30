// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The user.
    /// </summary>
    public class User : IEntity<UserDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly UserDocument innerDocument;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="firstRole">
        /// The first role.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public User(string userName, string password, string email, UserRoles firstRole, bool isLocked, Guid publicKey)
        {
            this.innerDocument = new UserDocument { UserName = userName, PublicKey = publicKey };
            this.ChangeEmail(email);
            this.ChangePassword(password);
            this.innerDocument.Roles.Add(firstRole);
            this.ChangeLockStatus(isLocked);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="firstRole">
        /// The first role.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        /// <param name="supervisor">
        /// The supervisor.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public User(
            string userName, 
            string password, 
            string email, 
            UserRoles firstRole, 
            bool isLocked, 
            UserLight supervisor, 
            Guid publicKey)
            : this(userName, password, email, firstRole, isLocked, publicKey)
        {
            if (supervisor == null)
            {
                throw new ArgumentNullException("Supervisor can't be null!");
            }

            this.SetSupervisor(supervisor);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="innerDocument">
        /// The inner document.
        /// </param>
        public User(UserDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public Guid UserId
        {
            get
            {
                return this.innerDocument.PublicKey;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add role.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <exception cref="DuplicateNameException">
        /// </exception>
        public void AddRole(UserRoles role)
        {
            if (this.innerDocument.Roles.Contains(role))
            {
                throw new DuplicateNameException(string.Format("user already in role {0}", role));
            }

            this.innerDocument.Roles.Add(role);
        }

        /// <summary>
        /// The change email.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void ChangeEmail(string email)
        {
            var emailValidator = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (!emailValidator.IsMatch(email))
            {
                throw new ArgumentException("email is invalid");
            }

            this.innerDocument.Email = email;
        }

        /// <summary>
        /// The change lock status.
        /// </summary>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        public void ChangeLockStatus(bool isLocked)
        {
            this.innerDocument.IsLocked = isLocked;
        }

        /// <summary>
        /// The change password.
        /// </summary>
        /// <param name="password">
        /// The password.
        /// </param>
        public void ChangePassword(string password)
        {
            /*  Regex passwordValidatorToMd5Hash = new Regex(@"^[0-9a-f]{32}$");
              if (!passwordValidatorToMd5Hash.IsMatch(password))
                  throw new ArgumentException("passwrod is invalid");*/
            this.innerDocument.Password = password;
        }

        /// <summary>
        /// The change role list.
        /// </summary>
        /// <param name="newRoleList">
        /// The new role list.
        /// </param>
        public void ChangeRoleList(IEnumerable<UserRoles> newRoleList)
        {
            this.innerDocument.Roles = newRoleList.ToList();
        }

        /// <summary>
        /// The clear supervisor.
        /// </summary>
        public void ClearSupervisor()
        {
            this.innerDocument.Supervisor = null;
        }

        /// <summary>
        /// The create supervisor.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.SubEntities.UserLight.
        /// </returns>
        public UserLight CreateSupervisor()
        {
            if (this.IsUserInRole(UserRoles.Supervisor))
            {
                return new UserLight(this.UserId, this.innerDocument.UserName);
            }

            return null;
        }

        /// <summary>
        /// The delete user.
        /// </summary>
        public void DeleteUser()
        {
            this.innerDocument.IsDeleted = true;
        }

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.UserDocument.
        /// </returns>
        public UserDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        /// <summary>
        /// The is user in role.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool IsUserInRole(UserRoles role)
        {
            return this.innerDocument.Roles.Contains(role);
        }

        /// <summary>
        /// The remove role.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool RemoveRole(UserRoles role)
        {
            return this.innerDocument.Roles.Remove(role);
        }

        /// <summary>
        /// The set locaton.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void SetLocaton(Location location)
        {
            // this.innerDocument.Location=((IEntity<LocationDocument>)location).GetInnerDocument();
        }

        /// <summary>
        /// The set supervisor.
        /// </summary>
        /// <param name="supervisor">
        /// The supervisor.
        /// </param>
        public void SetSupervisor(UserLight supervisor)
        {
            this.innerDocument.Supervisor = supervisor;
        }

        #endregion
    }
}