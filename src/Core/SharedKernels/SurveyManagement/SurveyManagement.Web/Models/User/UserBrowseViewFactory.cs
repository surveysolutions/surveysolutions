using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
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
            int count = this.documentItemSession.Query(queryableItems => queryableItems.Count());
            if (count == 0)
                return new UserBrowseView(input.Page, input.PageSize, count, new UserBrowseItem[0]);

            IEnumerable<UserDocument> query =
                documentItemSession.Query(_ => _.Where(u => input.Expression(u)))
                    .ToList()
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(
                        input.PageSize);
            UserBrowseItem[] items = query.Select(x => new UserBrowseItem(x.PublicKey, x.UserName, x.Email, x.CreationDate, x.IsLockedBySupervisor, x.IsLockedByHQ, x.Supervisor)).ToArray();
            return new UserBrowseView(input.Page, input.PageSize, count, items.ToArray());
        }

        #endregion
    }
}