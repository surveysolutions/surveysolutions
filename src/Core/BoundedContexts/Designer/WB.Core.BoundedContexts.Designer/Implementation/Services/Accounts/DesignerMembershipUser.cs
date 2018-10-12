using System;
using System.Web.Security;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts
{
    public class DesignerMembershipUser : MembershipUser
    {
        protected DesignerMembershipUser() { }

        public DesignerMembershipUser(
            string providerName,
            string name,
            object providerUserKey,
            string email,
            string passwordQuestion,
            string comment,
            bool isApproved,
            bool isLockedOut,
            DateTime creationDate,
            DateTime lastLoginDate,
            DateTime lastActivityDate,
            DateTime lastPasswordChangedDate,
            DateTime lastLockoutDate,
            bool canImportOnHq,
            string fullName)
            : base(providerName,
                name,
                providerUserKey,
                email,
                passwordQuestion,
                comment,
                isApproved,
                isLockedOut,
                creationDate,
                lastLoginDate,
                lastActivityDate,
                lastPasswordChangedDate,
                lastLockoutDate)
        {
            this.CanImportOnHq = canImportOnHq;
            this.FullName = fullName;
        }

        public virtual string FullName { get; set; }

        public virtual bool CanImportOnHq { get; set; }
    }
}
