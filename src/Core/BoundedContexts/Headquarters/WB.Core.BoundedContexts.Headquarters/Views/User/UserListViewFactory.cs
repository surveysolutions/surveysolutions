using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListViewFactory : IUserListViewFactory
    {
        private readonly IIdentityManager identityManager;

        public UserListViewFactory(IIdentityManager identityManager)
        {
            this.identityManager = identityManager;
        }

        public UserListView Load(UserListViewInputModel input)
        {
            input.Order = string.IsNullOrEmpty(input.Order) ? nameof(ApplicationUser.UserName) : input.Order;

            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query = _ => ApplyFilter(_, input);

            Func<IQueryable<ApplicationUser>, List<ApplicationUser>> pagedAndOrderedQuery = _ =>
            {
                _ = query(_).OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);

                return _.ToList();
            };


            var users = pagedAndOrderedQuery.Invoke(this.identityManager.Users).Select(x => new UserListItem(
                id: x.Id,
                creationDate: x.CreationDate,
                email: x.Email,
                isLockedBySupervisor: x.IsLockedBySupervisor,
                isLockedByHQ: x.IsLockedByHeadquaters,
                name: x.UserName,
                roles: x.Roles.Select(role => (UserRoles)Enum.Parse(typeof(UserRoles), role.RoleId)).ToList(),
                deviceId: x.DeviceId
                ));


            var totalCount = query.Invoke(this.identityManager.Users).Count();

            return new UserListView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = users.ToList()
            };
        }

        private static IQueryable<ApplicationUser> ApplyFilter(IQueryable<ApplicationUser> _, UserListViewInputModel input)
        {
            var allUsers = _.Where(x =>x.IsArchived==input.Archived && x.Roles.FirstOrDefault().RoleId == ((int)input.Role).ToString());
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                var searchByToLower = input.SearchBy.ToLower();
                allUsers = allUsers.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
            }
            return allUsers;
        }
    }
}