using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user browse view factory.
    /// </summary>
    public class UserBrowseViewFactory : IViewFactory<UserBrowseInputModel, UserBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<UserDocument> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public UserBrowseViewFactory(IQueryableReadSideRepositoryReader<UserDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
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
        /// The RavenQuestionnaire.Core.Views.User.UserBrowseView.
        /// </returns>
        public UserBrowseView Load(UserBrowseInputModel input)
        {
            return this.documentItemSession.Query(queryableItems =>
            {
                int count = queryableItems.Count();
                if (count == 0)
                    return new UserBrowseView(input.Page, input.PageSize, count, new UserBrowseItem[0]);

                // Perform the paged query
                #warning ReadLayer: ToList
                IEnumerable<UserDocument> query =
                    queryableItems.ToList().Where(input.Expression).Skip((input.Page - 1) * input.PageSize).Take(
                        input.PageSize);

                // And enact this query
                UserBrowseItem[] items = query.Select(x => new UserBrowseItem(x.PublicKey, x.UserName, x.Email,
                    x.CreationDate, x.IsLocked, x.Supervisor, x.Location.Location)).ToArray();
                return new UserBrowseView(input.Page, input.PageSize, count, items.ToArray());
            });
        }

        #endregion
    }
}