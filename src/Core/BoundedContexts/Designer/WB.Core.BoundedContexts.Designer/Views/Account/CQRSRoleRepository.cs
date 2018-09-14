using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    [ExcludeFromCodeCoverage]
    public class CQRSRoleRepository : IRoleRepository
    {
        private readonly ICommandService commandService;
        private readonly IAccountListViewFactory accountListViewFactory;
        private readonly IAccountViewFactory accountViewFactory;

        public CQRSRoleRepository(ICommandService commandService, IAccountListViewFactory accountListViewFactory, IAccountViewFactory accountViewFactory)
        {
            this.commandService = commandService;
            this.accountListViewFactory = accountListViewFactory;
            this.accountViewFactory = accountViewFactory;
        }

        public IUserWithRoles GetUser(string applicationName, string username)
        {
            return this.GetUser(username);
        }

        public void CreateRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void AddUserToRole(string applicationName, string roleName, Guid userid)
        {
            this.commandService.Execute(new AssignUserRole(userId: userid,
                                                                role: this.GetRoleByRoleName(roleName)));
        }

        public void RemoveUserFromRole(string applicationName, string roleName, string username)
        {
            var user = this.GetUser(username);
            this.commandService.Execute(new RemoveUserRole(userId: user.ProviderUserKey,
                                                                     role: this.GetRoleByRoleName(roleName)));
        }

        public IEnumerable<string> GetRoleNames(string applicationName)
        {
            return Enum.GetNames(typeof (SimpleRoleEnum));
        }

        public bool Exists(string applicationName, string roleName)
        {
            SimpleRoleEnum roleType;
            return Enum.TryParse(roleName, true, out roleType);
        }

        public int GetNumberOfUsersInRole(string applicationName, string roleName)
        {
            return this.GetUsersInRole(applicationName, roleName).Count();
        }

        public IEnumerable<string> FindUsersInRole(string applicationName, string roleName, string userNameToMatch)
        {
            var accounts =
                this.accountListViewFactory.Load(new AccountListViewInputModel()
                {
                    Role = this.GetRoleByRoleName(roleName),
                    Name = userNameToMatch
                });
            return accounts.Items.Select(x => x.UserName);
        }

        public IEnumerable<string> GetUsersInRole(string applicationName, string roleName)
        {
            var accounts =
                this.accountListViewFactory.Load(new AccountListViewInputModel()
                    {
                        Role = this.GetRoleByRoleName(roleName)
                    });
            return accounts.Items.Select(x => x.UserName);
        }

        private IAccountView GetUser(string username)
        {
            return this.accountViewFactory.Load(new AccountViewInputModel(
                accountName: username,
                accountEmail: null,
                confirmationToken: null,
                resetPasswordToken:null));
        }

        private SimpleRoleEnum GetRoleByRoleName(string roleName)
        {
            var role = SimpleRoleEnum.User;
            Enum.TryParse(roleName, out role);
            return role;
        }
    }
}
