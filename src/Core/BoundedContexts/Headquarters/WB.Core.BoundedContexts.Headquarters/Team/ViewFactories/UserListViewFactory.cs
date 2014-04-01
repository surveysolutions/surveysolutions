using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Team.ViewFactories
{
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
                                            {
                                            UserId = x.PublicKey,
                                            CreationDate = x.CreationDate.ToShortDateString(),
                                            Email = x.Email,
                                            IsLocked = x.IsLocked,
                                            UserName = x.UserName,
                                            Roles = x.Roles
                                            });

                        return new UserListView {Page = input.Page, PageSize = input.PageSize, TotalCount = all.Count(), Items = selection};
                    });
        }
    }
}