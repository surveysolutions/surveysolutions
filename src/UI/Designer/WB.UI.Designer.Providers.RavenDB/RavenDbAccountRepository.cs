using WB.UI.Designer.Providers.Membership;
using WB.UI.Designer.Providers.Repositories.RavenDb;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

namespace WB.UI.Designer.Providers.RavenDB
{
    using WB.UI.Designer.Providers.Membership;
    using WB.UI.Designer.Providers.Repositories.RavenDb;

    /// <summary>
    /// Raven implementation of the account repository
    /// </summary>
    public class RavenDbAccountRepository : IAccountRepository
    {
        private readonly IDocumentSession _documentSession;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDbAccountRepository"/> class.
        /// </summary>
        /// <param name="documentSession">The document session.</param>
        public RavenDbAccountRepository(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        #region Implementation of IAccountRepository

        /// <summary>
        /// Gets whether all users must have unique email addresses.
        /// </summary>
        public bool IsUniqueEmailRequired { get; set; }

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="account">Acount to register</param>
        /// <returns>Result indication</returns>
        /// <remarks>
        /// Implementations should set the <see cref="IMembershipAccount.ProviderUserKey"/> property before returning.
        /// </remarks>
        public MembershipCreateStatus Register(IMembershipAccount account)
        {
            var doc = account as AccountDocument ?? new AccountDocument(account);
            _documentSession.Store(doc);

            return MembershipCreateStatus.Success;
        }

        /// <summary>
        /// Fetch a user from the service.
        /// </summary>
        /// <param name="username">Unique user name</param>
        /// <returns>User if found; otherwise null.</returns>
        public IMembershipAccount Get(string username)
        {
            return _documentSession.Query<AccountDocument>().FirstOrDefault(user => user.UserName == username);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="account">Account being updated.</param>
        public void Update(IMembershipAccount account, MembershipEventType eventType)
        {
            AccountDocument accountDocument;
            if (!(account is AccountDocument))
            {
                accountDocument = _documentSession.Query<AccountDocument>().Single(m => m.UserName == account.UserName);
                if (accountDocument == null)
                    throw new InvalidOperationException("Account " + account + " is not a valid raven account.");

                accountDocument.Copy(account);
            }
            else
                accountDocument = (AccountDocument) account;


            _documentSession.Store(accountDocument);
        }

        /// <summary>
        /// Get a user by using the implementation specific (your) Id.
        /// </summary>
        /// <param name="id">User identity specific for each account repository implementation</param>
        /// <returns>User if found; otherwise null.</returns>
        public IMembershipAccount GetByProviderKey(object id)
        {
            if (id == null) throw new ArgumentNullException("id");

            return _documentSession.Query<AccountDocument>().FirstOrDefault(user => user.ProviderUserKey == id);
        }

        /// <summary>
        /// Translate an email into a user name.
        /// </summary>
        /// <param name="email">Email to lookup</param>
        /// <returns>User name if the specified email was found; otherwise null.</returns>
        public string GetUserNameByEmail(string email)
        {
            if (email == null) throw new ArgumentNullException("email");

            return
                _documentSession.Query<AccountDocument>().Where(user => user.Email == email).Select(
                    user => user.UserName).FirstOrDefault();
        }

        /// <summary>
        /// Delete a user from the database.
        /// </summary>
        /// <param name="username">Unique user name</param>
        /// <param name="deleteAllRelatedData">Delete information from all other tables etc</param>
        /// <returns>true if was removed successfully; otherwise false.</returns>
        public bool Delete(string username, bool deleteAllRelatedData)
        {
            var dbUser = _documentSession.Query<AccountDocument>().SingleOrDefault(user => user.UserName == username);
            if (dbUser == null)
                return true;

            _documentSession.Delete(dbUser);

            try
            {
                Deleted(this, new DeletedEventArgs(dbUser));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get number of users that are online
        /// </summary>
        /// <returns>Number of online users</returns>
        public int GetNumberOfUsersOnline()
        {
            return _documentSession.Query<AccountDocument>().Count(user => user.IsOnline);
        }

        /// <summary>
        /// Find all users
        /// </summary>
        /// <param name="pageIndex">One based index</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <param name="totalRecords">Total number of users</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        public IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords)
        {
            IQueryable<AccountDocument> query = _documentSession.Query<AccountDocument>();
            query = CountAndPageQuery(pageIndex, pageSize, out totalRecords, query);
            return query.ToList();
        }

        /// <summary>
        /// Find new acounts that haven't been activated.
        /// </summary>
        /// <param name="pageIndex">zero based index</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <param name="totalRecords">Total number of users</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        public IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords)
        {
            var query = _documentSession.Query<AccountDocument>().Where(p => p.IsConfirmed == false);
            query = CountAndPageQuery(pageIndex, pageSize, out totalRecords, query);
            return query.ToList();
        }

        /// <summary>
        /// Find by searching for user name
        /// </summary>
        /// <param name="usernameToMatch">User name (or partial user name)</param>
        /// <param name="pageIndex">Zero based index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalRecords">total number of records that partially matched the specified user name</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        public IEnumerable<IMembershipAccount> FindByUserName(string usernameToMatch, int pageIndex, int pageSize,
                                                              out int totalRecords)
        {
            var query = _documentSession.Query<AccountDocument>().Where(user => user.UserName.Contains(usernameToMatch));
            query = CountAndPageQuery(pageIndex, pageSize, out totalRecords, query);
            return query.ToList();
        }

        /// <summary>
        /// Find by searching for the specified email
        /// </summary>
        /// <param name="emailToMatch">Number of users that have the specified email (no partial matches)</param>
        /// <param name="pageIndex">Zero based index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalRecords">total number of records that matched the specified email</param>
        /// <returns>A collection of users or an empty collection if no users was found.</returns>
        public IEnumerable<IMembershipAccount> FindByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                           out int totalRecords)
        {
            var query = _documentSession.Query<AccountDocument>().Where(user => user.Email == emailToMatch);
            query = CountAndPageQuery(pageIndex, pageSize, out totalRecords, query);
            return query.ToList();
        }

        /// <summary>
        /// Create a new membership account
        /// </summary>
        /// <param name="providerUserKey">Primary key in the data source</param>
        /// <param name="applicationName">Name of the application that the account is created for</param>
        /// <param name="username">User name</param>
        /// <param name="email">Email address</param>
        /// <returns>
        /// Created account
        /// </returns>
        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            var account = new AccountDocument
                              {
                                  ApplicationName = applicationName,
                                  UserName = username,
                                  Email = email,
                                  ProviderUserKey = providerUserKey,
                                  CreatedAt = DateTime.Now
                              };
            return account;
        }

        public IMembershipAccount GetUserByResetPasswordToken(string token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when a member has been deleted.
        /// </summary>
        public event EventHandler<DeletedEventArgs> Deleted = delegate { };

        private IQueryable<AccountDocument> CountAndPageQuery(int pageIndex, int pageSize, out int totalRecords,
                                                              IQueryable<AccountDocument> query)
        {
            totalRecords = query.Count();
            query = pageIndex == 1
                        ? _documentSession.Query<AccountDocument>().Take(pageSize)
                        : _documentSession.Query<AccountDocument>().Skip((pageIndex - 1)*pageSize).Take(pageSize);
            return query;
        }

        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            var _user = _documentSession.Query<AccountDocument>().FirstOrDefault(user =>
                                                                                user.ConfirmationToken == confirmationToken);
            return _user == null ? string.Empty : _user.UserName;
        }
        #endregion
    }
}