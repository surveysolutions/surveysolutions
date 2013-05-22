using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.UI.Designer.Providers.CQRS
{
    using WB.UI.Designer.Providers.CQRS.Accounts.Commands;
    using WB.UI.Designer.Providers.CQRS.Accounts.View;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class CQRSRoleRepository : IRoleRepository
    {
        private readonly IViewRepository _viewRepository;
        private readonly ICommandService _commandService;

        public CQRSRoleRepository(IViewRepository viewRepository, ICommandService commandService)
        {
            _viewRepository = viewRepository;
            _commandService = commandService;
        }

        public IUserWithRoles GetUser(string applicationName, string username)
        {
            return GetUser(username);
        }

        public void CreateRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void AddUserToRole(string applicationName, string roleName, string username)
        {
            var user = GetUser(username);
            _commandService.Execute(new AddRoleToAccountCommnad(accountPublicKey: user.PublicKey,
                                                                role: GetRoleByRoleName(roleName)));
        }

        public void RemoveUserFromRole(string applicationName, string roleName, string username)
        {
            var user = GetUser(username);
            _commandService.Execute(new RemoveRoleFromAccountCommnad(accountPublicKey: user.PublicKey,
                                                                     role: GetRoleByRoleName(roleName)));
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
            return GetUsersInRole(applicationName, roleName).Count();
        }

        public IEnumerable<string> FindUsersInRole(string applicationName, string roleName, string userNameToMatch)
        {
            var accounts =
                _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
                {
                    Role = GetRoleByRoleName(roleName),
                    Name = userNameToMatch
                });
            return accounts.Items.Select(x => x.UserName);
        }

        public IEnumerable<string> GetUsersInRole(string applicationName, string roleName)
        {
            var accounts =
                _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
                    {
                        Role = GetRoleByRoleName(roleName)
                    });
            return accounts.Items.Select(x => x.UserName);
        }

        private AccountView GetUser(string username)
        {
            return _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(
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
