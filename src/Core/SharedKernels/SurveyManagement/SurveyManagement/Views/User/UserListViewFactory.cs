using System.Linq;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UserListViewFactory : IUserListViewFactory
    {
        private readonly IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor;

        public UserListViewFactory(IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor)
        {
            this.readSideRepositoryIndexAccessor = readSideRepositoryIndexAccessor;
        }

        public UserListView Load(UserListViewInputModel input)
        {
            string indexName = typeof (UserDocumentsByBriefFields).Name;

            var allUsers = this.readSideRepositoryIndexAccessor.Query<UserDocument>(indexName);
                                                               

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                allUsers = allUsers.Search(x => x.UserName, input.SearchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
                                   .Search(x => x.Email, input.SearchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards);
            }

            allUsers = allUsers.Where(x => x.Roles.Contains(input.Role));

            var users = allUsers.OrderUsingSortExpression(input.Order)
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize)
                .ToList()
                .Select(x => new UserListItem(
                    id: x.PublicKey,
                    creationDate: x.CreationDate,
                    email: x.Email,
                    isLockedBySupervisor: x.IsLockedBySupervisor,
                    isLockedByHQ: x.IsLockedByHQ,
                    name: x.UserName,
                                            roles: x.Roles.ToList(),
                                            deviceId:x.DeviceId
                    ));

            return new UserListView
            {
                Page = input.Page, 
                PageSize = input.PageSize, 
                TotalCount = allUsers.Count(), 
                Items = users.ToList()
            };
        }
    }
}