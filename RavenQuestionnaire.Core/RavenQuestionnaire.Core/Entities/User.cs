using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
    public class User : IEntity<UserDocument>
    {
        private UserDocument innerDocument;

        public string UserId { get { return innerDocument.Id; } }

        public User(string userName, string password, string email, UserRoles firstRole, bool isLocked)
        {
            innerDocument = new UserDocument() { UserName = userName };
            ChangeEmail(email);
            ChangePassword(password);
            innerDocument.Roles.Add(firstRole);
            ChangeLockStatus(isLocked);
        }
        public User(string userName, string password, string email, UserRoles firstRole, bool isLocked, UserLight supervisor)
            : this(userName, password, email, firstRole, isLocked)
        {
            if (supervisor == null)
                throw new ArgumentNullException("supervisor can't be null");
            SetSupervisor(supervisor);
        }

        public User(UserDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        public void SetLocaton(Location location)
        {
            this.innerDocument.Location=((IEntity<LocationDocument>)location).GetInnerDocument();
        }

        public UserLight CreateSupervisor()
        {
            if (IsUserInRole(UserRoles.Supervisor))
                return new UserLight(UserId, innerDocument.UserName );

            return null;
        }

        public void SetSupervisor(UserLight supervisor)
        {
            this.innerDocument.Supervisor = supervisor;
        }
        public void ClearSupervisor()
        {
            this.innerDocument.Supervisor = null;
        }

        public void ChangeLockStatus(bool isLocked)
        {
            this.innerDocument.IsLocked = isLocked;
        }
        public void DeleteUser()
        {
            this.innerDocument.IsDeleted = true;
        }

        public void AddRole(UserRoles role)
        {
            if (innerDocument.Roles.Contains(role))
                throw new DuplicateNameException(string.Format("user already in role {0}", role));
            innerDocument.Roles.Add(role);
        }
        public void ChangeRoleList(IEnumerable<UserRoles> newRoleList)
        {
            innerDocument.Roles = newRoleList.ToList();
        }
        public bool IsUserInRole(UserRoles role)
        {
            return innerDocument.Roles.Contains(role);
        }

        public bool RemoveRole(UserRoles role)
        {
            return innerDocument.Roles.Remove(role);
        }

        public void ChangeEmail(string email)
        {
            Regex emailValidator = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (!emailValidator.IsMatch(email))
                throw new ArgumentException("email is invalid");

            innerDocument.Email = email;
        }

        public void ChangePassword(string password)
        {
            /*  Regex passwordValidatorToMd5Hash = new Regex(@"^[0-9a-f]{32}$");
              if (!passwordValidatorToMd5Hash.IsMatch(password))
                  throw new ArgumentException("passwrod is invalid");*/
            innerDocument.Password = password;
        }

        UserDocument IEntity<UserDocument>.GetInnerDocument()
        {
            return innerDocument;
        }
    }
}
