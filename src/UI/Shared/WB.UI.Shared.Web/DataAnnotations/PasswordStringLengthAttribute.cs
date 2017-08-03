using System.ComponentModel.DataAnnotations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Shared.Web.DataAnnotations
{
    public class PasswordStringLengthAttribute : StringLengthAttribute
    {
        public PasswordStringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
            base.MinimumLength = this.MinimumLength;
        }

        public new int MinimumLength
        {
            get
            {
                return this.passwordPolicy.PasswordMinimumLength;
            }
        }

        private IPasswordPolicy passwordPolicy
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IPasswordPolicy>();
            }
        }
    }
}