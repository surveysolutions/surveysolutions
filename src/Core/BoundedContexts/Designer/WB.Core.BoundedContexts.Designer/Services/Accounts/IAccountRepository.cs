using System.Collections.Generic;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;

namespace WB.Core.BoundedContexts.Designer.Services.Accounts
{
    /// <summary>
    /// Repository for user accounts
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>
        /// Gets whether all users must have unique email addresses.
        /// </summary>
        bool IsUniqueEmailRequired { get; }

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="account">Acount to register</param>
        /// <returns>Result indication</returns>
        /// <remarks>
        /// Implementations should set the <see cref="IMembershipAccount.ProviderUserKey"/> property before returning.
        /// </remarks>
        MembershipCreateStatus Register(IMembershipAccount account);

        /// <summary>
        /// Fetch a user from the service.
        /// </summary>
        /// <param name="username">Unique user name</param>
        /// <returns>User if found; otherwise null.</returns>
        IMembershipAccount Get(string username);

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="account">Account being updated.</param>
        /// <param name="eventType">Using only for event sourcing</param>
        void Update(IMembershipAccount account, MembershipEventType eventType = MembershipEventType.Update);

        /// <summary>
        /// Get a user by using your PK.
        /// </summary>
        /// <param name="id">PK in your own db</param>
        /// <returns>User if found; otherwise null.</returns>
        IMembershipAccount GetByProviderKey(object id);

        /// <summary>
        /// Translate an email into a user name.
        /// </summary>
        /// <param name="email">Email to lookup</param>
        /// <returns>User name if the specified email was found; otherwise null.</returns>
        string GetUserNameByEmail(string email);

        /// <summary>
        /// Translate an confirmation token into a user name.
        /// </summary>
        /// <param name="confirmationToken">Confirmation token to lookup</param>
        /// <returns>User name if the specified confirmation token was found; otherwise null.</returns>
        string GetUserNameByConfirmationToken(string confirmationToken);

        /// <summary>
        /// Delete a user from the database.
        /// </summary>
        /// <param name="username">Unique user name</param>
        /// <returns>true if was removed successfully; otherwise false.</returns>
        bool Delete(string username);

        /// <summary>
        /// Get number of users that are online
        /// </summary>
        /// <returns>Number of online users</returns>
        int GetNumberOfUsersOnline();

        /// <summary>
        /// Find all users
        /// </summary>
        /// <param name="pageIndex">zero based index</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <param name="totalRecords">Total number of users</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords);

        /// <summary>
        /// Find new acounts that haven't been activated.
        /// </summary>
        /// <param name="pageIndex">zero based index</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <param name="totalRecords">Total number of users</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords);

        /// <summary>
        /// Find by searching for user name
        /// </summary>
        /// <param name="usernameToMatch">User name (or partial user name)</param>
        /// <param name="pageIndex">Zero based index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalRecords">total number of records that partially matched the specified user name</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        IEnumerable<IMembershipAccount> FindByUserName(string usernameToMatch, int pageIndex, int pageSize,
                                                       out int totalRecords);

        /// <summary>
        /// Find by searching for the specified email
        /// </summary>
        /// <param name="emailToMatch">Number of users that have the specified email (no partial matches)</param>
        /// <param name="pageIndex">Zero based index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalRecords">total number of records that matched the specified email</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        IEnumerable<IMembershipAccount> FindByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                    out int totalRecords);

        /// <summary>
        /// Create a new membership account
        /// </summary>
        /// <param name="providerUserKey">Primary key in the data source</param>
        /// <param name="applicationName">Name of the application that the account is created for</param>
        /// <param name="username">User name</param>
        /// <param name="email">Email address</param>
        /// <returns>Created account</returns>
        IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email, string fullName);

        /// <summary>
        /// The get user by reset password token.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        IMembershipAccount GetUserByResetPasswordToken(string token);

        IMembershipAccount GetByNameOrEmail(string userNameOrEmail);
    }
}
