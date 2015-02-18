using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UserListViewFactory : IViewFactory<UserListViewInputModel, UserListView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> userStore;
        private readonly IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor;
        public UserListViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users, IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor)
        {
            this.userStore = users;
            this.readSideRepositoryIndexAccessor = readSideRepositoryIndexAccessor;
        }

        public UserListView Load(UserListViewInputModel input)
        {
            var allUsers = this.readSideRepositoryIndexAccessor.Query<UserDocumentBrief>(
                typeof (UserDocumentsByBriefFields).Name).Search(x => x.Roles, input.Role.ToString());
            var users = allUsers.OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize).ProjectFromIndexFieldsInto<UserDocument>().ToList().Select(
                        x =>
                            new UserListItem
                                (
                                id: x.PublicKey,
                                creationDate: x.CreationDate,
                                email: x.Email,
                                isLockedBySupervisor: x.IsLockedBySupervisor,
                                isLockedByHQ: x.IsLockedByHQ,
                                name: x.UserName,
                                roles: x.Roles
                                ));

            return new UserListView { Page = input.Page, PageSize = input.PageSize, TotalCount = allUsers.Count(), Items = users };
        }
    }
}