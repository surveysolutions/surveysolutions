﻿using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListViewFactory : IUserListViewFactory
    {
        private readonly IPlainStorageAccessor<UserDocument> readSideRepositoryIndexAccessor;

        public UserListViewFactory(IPlainStorageAccessor<UserDocument> readSideRepositoryIndexAccessor)
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
            var allUsers = _.Where(x =>x.IsArchived==input.Archived && x.Roles.Contains(input.Role));
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                var searchByToLower = input.SearchBy.ToLower();
                allUsers = allUsers.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
            }
            return allUsers;
        }
    }
}