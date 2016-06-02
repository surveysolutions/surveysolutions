using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;

namespace WB.UI.Headquarters.Models.User
{
    public interface IUserBrowseViewFactory
    {
        UserBrowseView Load(UserBrowseInputModel input);
    }

    public class UserBrowseViewFactory : IUserBrowseViewFactory
    {
        private readonly IPlainStorageAccessor<UserDocument> indexAccessor;

        public UserBrowseViewFactory(IPlainStorageAccessor<UserDocument> indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public UserBrowseView Load(UserBrowseInputModel input)
        {
            bool anyUserExists = this.indexAccessor.Query(_ => _.Any(x => !x.IsArchived));
            if (!anyUserExists)
            {
                return new UserBrowseView(input.Page, input.PageSize, 0, new UserBrowseItem[0]);
            }
            List<UserDocument> pagedQuery = this.indexAccessor.Query(_ => ApplyPaging(Filter(_, input), input).ToList());

            IEnumerable<UserBrowseItem> items = pagedQuery.Select(x => new UserBrowseItem(x.PublicKey, 
                                                                               x.UserName, 
                                                                               x.Email, 
                                                                               x.CreationDate, 
                                                                               x.IsLockedBySupervisor, 
                                                                               x.IsLockedByHQ, 
                                                                               x.Supervisor));

            var totalCount = this.indexAccessor.Query(_ => Filter(_, input).Count());

            return new UserBrowseView(input.Page, input.PageSize, totalCount, items);
        }

        private static IQueryable<UserDocument> ApplyPaging(IQueryable<UserDocument> query, UserBrowseInputModel input)
        {
            return query.Skip((input.Page - 1) * input.PageSize)
                .Take(input.PageSize);
        }

        private static IQueryable<UserDocument> Filter(IQueryable<UserDocument> _, UserBrowseInputModel input)
        {
            var query = _.Where(x => !x.IsArchived);
            if (input.Role.HasValue)
            {
                query = query.Where(x => x.Roles.Contains(input.Role.Value));
            }
            return query;
        }
    }
}