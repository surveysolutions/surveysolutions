
namespace WB.UI.Shared.Web.MembershipProvider.Accounts
{
    /// <summary>
    /// Extension methods for <seealso cref="IMembershipAccount"/>
    /// </summary>
    public static class PasswordExtensions
    {
        /// <summary>
        /// Create a new password info class.
        /// </summary>
        /// <param name="account">Account containing password information</param>
        /// <returns>Password info object</returns>
        public static AccountPasswordInfo CreatePasswordInfo(this IMembershipAccount account)
        {
            return new AccountPasswordInfo(account.UserName, account.Password) {PasswordSalt = account.PasswordSalt};
        }
    }
}