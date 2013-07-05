using System;
using System.Linq;
using Main.Core.Documents;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user view factory.
    /// </summary>
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
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
        public UserView Load(UserViewInputModel input)
        {
            return this.users.Query(queryableUsers =>
            {
                UserDocument doc = null;
                if (input.UserId != Guid.Empty)
                    doc = queryableUsers.FirstOrDefault(u => u.PublicKey == input.UserId);
                else
                    if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
                    {
                        doc =
                            queryableUsers.FirstOrDefault(
                                u => u.UserName == input.UserName);
                    }

                if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
                {
                    doc = queryableUsers.FirstOrDefault(u => u.UserName == input.UserName);
                    if (doc != null && doc.Password != input.Password)
                        return null;
                }

                if (doc == null || doc.IsDeleted)
                    return null;

                return new UserView(doc.PublicKey, doc.UserName, doc.Password, doc.Email,
                                    doc.CreationDate, doc.Roles, doc.IsLocked, doc.Supervisor, doc.Location.Id);
            });
        }

        #endregion
    }
}