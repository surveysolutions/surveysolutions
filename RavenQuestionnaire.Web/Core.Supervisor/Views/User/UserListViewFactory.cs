using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    public class UserListViewFactory : IViewFactory<UserListViewInputModel, UserListView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public UserListViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserListView Load(UserListViewInputModel input)
        {
            Func<UserDocument, bool> query =
                _ => !_.IsDeleted && (input.Role == UserRoles.Undefined || _.Roles.Contains(input.Role));

            return this.users.Query(
                _ =>
                    {
                        var all = _.Where(query).AsQueryable().OrderUsingSortExpression(input.Order);

                        var selection =
                            all.Skip((input.Page - 1)*input.PageSize)
                                .Take(input.PageSize)
                                .ToList()
                                .Select(
                                    x =>
                                        new UserListItem
                                            (
                                            id: x.PublicKey,
                                            creationDate: x.CreationDate,
                                            email: x.Email,
                                            isLocked: x.IsLocked,
                                            name: x.UserName,
                                            roles: x.Roles
                                            ));

                        return new UserListView {TotalCount = all.Count(), Items = selection};
                    });
        }
    }
}