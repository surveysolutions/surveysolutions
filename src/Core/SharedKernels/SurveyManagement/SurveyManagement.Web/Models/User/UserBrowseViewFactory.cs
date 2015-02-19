using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public class UserBrowseViewFactory : IViewFactory<UserBrowseInputModel, UserBrowseView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> documentItemSession;

        public UserBrowseViewFactory(IQueryableReadSideRepositoryReader<UserDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

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
    }
}