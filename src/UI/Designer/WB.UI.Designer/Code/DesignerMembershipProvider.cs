using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Services.Accounts;

namespace WB.UI.Designer.Code
{
    /// <summary>
    ///     The designer membership provider.
    /// </summary>
    public class DesignerMembershipProvider : MembershipProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The try get confirmation token by user name.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public string GetConfirmationTokenByUserName(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user != null)
            {
                return user.ConfirmationToken;
            }

            return string.Empty;
        }

        #endregion
    }
}
