using System;
using System.Linq;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Main.Core.View;

namespace Designer.Web.Providers.Repositories.CQRS
{
    /// <summary>
    /// The account view factory.
    /// </summary>
    public class AccountViewFactory : IViewFactory<AccountViewInputModel, AccountView>
    {
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public AccountViewFactory(IDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.User.UserView.
        /// </returns>
        public AccountView Load(AccountViewInputModel input)
        {
            //UserDocument doc = null;
            //if (input.AccountId != Guid.Empty)
            //    doc = this.users.Query().FirstOrDefault(u => u.PublicKey == input.AccountId);
            //else
            //    if (!string.IsNullOrEmpty(input.AccountName) && string.IsNullOrEmpty(input.Password))
            //    {
            //        doc =
            //            this.users.Query().FirstOrDefault(
            //                u => string.Compare(u.UserName, input.AccountName, StringComparison.OrdinalIgnoreCase) == 0);
            //    }

            //if (!string.IsNullOrEmpty(input.AccountName) && !string.IsNullOrEmpty(input.Password))
            //{
            //    doc = this.users.Query().FirstOrDefault(u => u.UserName == input.AccountName);
            //    if (doc != null && doc.Password != input.Password)
            //        return null;
            //}

            //if (doc == null || doc.IsDeleted)
            //    return null;

            //return new AccountView(doc.PublicKey, doc.UserName, doc.Password, doc.Email,
            //                    doc.CreationDate, doc.Roles, doc.IsLocked, doc.Supervisor, doc.Location.Id);

            return new AccountView();
        }

        #endregion
    }
}