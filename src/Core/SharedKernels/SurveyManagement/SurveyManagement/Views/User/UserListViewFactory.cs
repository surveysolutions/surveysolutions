using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UserListViewFactory : IUserListViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> readSideRepositoryIndexAccessor;

        public UserListViewFactory(IQueryableReadSideRepositoryReader<UserDocument> readSideRepositoryIndexAccessor)
        {
            this.readSideRepositoryIndexAccessor = readSideRepositoryIndexAccessor;
        }

        public UserListView Load(UserListViewInputModel input)
        {
            var users = this.readSideRepositoryIndexAccessor.Query(_ =>
            {
                var allUsers = ApplyFilter(_, input);

                allUsers = allUsers.OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);

                return allUsers.ToList();
            }).Select(x => new UserListItem(
                        id: x.PublicKey,
                        creationDate: x.CreationDate,
                        email: x.Email,
                        isLockedBySupervisor: x.IsLockedBySupervisor,
                        isLockedByHQ: x.IsLockedByHQ,
                        name: x.UserName,
                        roles: x.Roles.ToList(),
                        deviceId: x.DeviceId
                        ));


            var totalCount = this.readSideRepositoryIndexAccessor.Query(_ => ApplyFilter(_, input)).Count();

            return new UserListView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = users.ToList()
            };
        }

        private static IQueryable<UserDocument> ApplyFilter(IQueryable<UserDocument> _, UserListViewInputModel input)
        {
            var allUsers = _.Where(x => x.Roles.Contains(input.Role));
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                allUsers = allUsers.Where(x => x.UserName.Contains(input.SearchBy) || x.Email.Contains(input.SearchBy));
            }
            return allUsers;
        }
    }
}