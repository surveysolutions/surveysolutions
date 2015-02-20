using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public class UserBrowseViewFactory : IViewFactory<UserBrowseInputModel, UserBrowseView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public UserBrowseViewFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public UserBrowseView Load(UserBrowseInputModel input)
        {
            var indexName = typeof (UserDocumentsByBriefFields).Name;

            bool anyUserExists = this.indexAccessor.Query<UserDocument>(indexName).Any(x => !x.IsDeleted);
            if (!anyUserExists)
            {
                return new UserBrowseView(input.Page, input.PageSize, 0, new UserBrowseItem[0]);
            }

            var query = this.indexAccessor.Query<UserDocument>(indexName)
                                          .Where(x => !x.IsDeleted);

            if (input.Role.HasValue)
            {
                query = query.Where(x => x.Roles.Contains(input.Role.Value));
            }

            var pagedQuery = query.Skip((input.Page - 1) * input.PageSize)
                                  .Take(input.PageSize);
                
            UserBrowseItem[] items = pagedQuery.ToList()
                                               .Select(x => new UserBrowseItem(x.PublicKey, 
                                                                               x.UserName, 
                                                                               x.Email, 
                                                                               x.CreationDate, 
                                                                               x.IsLockedBySupervisor, 
                                                                               x.IsLockedByHQ, 
                                                                               x.Supervisor))
                                               .ToArray();

            return new UserBrowseView(input.Page, input.PageSize, query.Count(), items);
        }
    }
}