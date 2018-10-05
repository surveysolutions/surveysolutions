using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public class User : IPlainAggregateRoot, IAccountView
    {
        #region Properties

        public virtual string ApplicationName { get; set; }

        public virtual string Comment { get; set; }

        public virtual string ConfirmationToken { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string Email { get; set; }

        public virtual bool IsConfirmed { get; set; }

        public virtual bool IsLockedOut { get; set; }

        public virtual bool CanImportOnHq { get; set; }

        public virtual bool IsOnline { get; set; }

        public virtual DateTime LastActivityAt { get; set; }

        public virtual DateTime LastLockedOutAt { get; set; }

        public virtual DateTime LastLoginAt { get; set; }

        public virtual DateTime LastPasswordChangeAt { get; set; }

        public virtual string Password { get; set; }

        public virtual string PasswordAnswer { get; set; }

        public virtual string PasswordQuestion { get; set; }

        public virtual DateTime PasswordResetExpirationDate { get; set; }

        public virtual string PasswordResetToken { get; set; }

        public virtual string PasswordSalt { get; set; }

        public virtual string UserId { get; set; }

        public virtual string FullName { get; set; }

        public virtual Guid ProviderUserKey
        {
            get { return providerUserKey; }
            set
            {
                this.UserId = value.FormatGuid();
                providerUserKey = value;
            }
        }

        private Guid providerUserKey;

        public virtual string UserName { get; set; }

        public virtual ISet<SimpleRoleEnum> SimpleRoles { get; set; } = new HashSet<SimpleRoleEnum>();

        #endregion

        public virtual Guid Id => this.ProviderUserKey;

        public virtual void SetId(Guid id) => this.ProviderUserKey = id;

        public virtual void Register(string applicationName, string userName, string email, Guid accountId,
            string password, string passwordSalt, bool isConfirmed, 
            string confirmationToken, string fullName)
        {
            this.UserName = GetNormalizedUserName(userName);
            this.Email = email;
            this.ConfirmationToken = confirmationToken;
            this.ApplicationName = applicationName;
            this.CreatedAt = DateTime.UtcNow;
            this.FullName = fullName;

            this.ResetPassword(password: password, passwordSalt: passwordSalt);
            if (isConfirmed)
            {
                this.Confirm();
            }
        }

        public virtual void AddRole(SimpleRoleEnum role)
        {
            this.SimpleRoles.Add(role);
        }

        public virtual void ChangePassword(string password)
        {
            this.Password = password;
            this.LastPasswordChangeAt = DateTime.UtcNow;
        }

        public virtual void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            this.PasswordAnswer = passwordAnswer;
            this.PasswordQuestion = passwordQuestion;
        }

        public virtual void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            this.PasswordResetToken = passwordResetToken;
            this.PasswordResetExpirationDate = passwordResetExpirationDate;
        }

        public virtual void Confirm()
        {
            this.IsConfirmed = true;
        }

        public virtual void Delete()
        {
        }

        public virtual void Lock()
        {
            this.IsLockedOut = true;
            this.LastLockedOutAt = DateTime.UtcNow;
        }

        public virtual void Unlock()
        {
            this.IsLockedOut = false;
        }

        public virtual void LoginFailed()
        {
        }
        
        public virtual void RemoveRole(SimpleRoleEnum role)
        {
            this.SimpleRoles.Remove(role);
        }

        public virtual void ResetPassword(string password, string passwordSalt)
        {
            this.PasswordSalt = passwordSalt;
            this.Password = password;
        }

        public virtual void Update(string userName, bool isLockedOut, string passwordQuestion, string email, bool isConfirmed, string comment, bool canImportOnHq, string fullName)
        {
            this.Comment = comment;
            this.Email = email;
            this.PasswordQuestion = passwordQuestion;
            this.UserName = GetNormalizedUserName(userName);
            this.CanImportOnHq = canImportOnHq;
            this.FullName = fullName;

            if (!this.IsConfirmed && isConfirmed)
            {
                this.IsConfirmed = true;
            }

            if (this.IsLockedOut != isLockedOut)
            {
                if (isLockedOut)
                {
                    this.IsLockedOut = true;
                    this.LastLockedOutAt = DateTime.UtcNow;
                }
                else
                {
                    this.IsLockedOut = false;
                }
            }
        }

        public virtual bool IsInRole(string roleName)
        {
            return this.GetRoles().Contains(roleName);
        }

        public virtual IEnumerable<string> GetRoles()
        {
            return this.SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x));
        }

        private static string GetNormalizedUserName(string userName) => userName.ToLower();
    }
}
