#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserManager : UserManager<HqUser>
    {
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly ISystemLog systemLog;
        private readonly IAuthorizedUser authorizedUser;

        public HqUserManager(IUserStore<HqUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<HqUser> passwordHasher, 
            IEnumerable<IUserValidator<HqUser>> userValidators, 
            IEnumerable<IPasswordValidator<HqUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<HqUser>> logger,
            IWorkspacesService workspacesService,
            IWorkspaceContextAccessor workspaceContextAccessor,
            ISystemLog systemLog,
            IAuthorizedUser authorizedUser) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.workspacesService = workspacesService;
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.systemLog = systemLog;
            this.authorizedUser = authorizedUser;
        }

        public override async Task<IdentityResult> CreateAsync(HqUser user)
        {
            var workspace = this.workspaceContextAccessor.CurrentWorkspace();

            var result = await base.CreateAsync(user);

            if (result.Succeeded && !user.IsInRole(UserRoles.Administrator))
            {
                if (workspace != null)
                {
                    this.workspacesService.AddUserToWorkspace(user, workspace.Name);
                }
            }

            return result;
        }

        public override Task<bool> CheckPasswordAsync(HqUser user, string password)
        {
            return base.CheckPasswordAsync(user, password);
        }

        public override async Task<IdentityResult> ResetPasswordAsync(HqUser user, string token, string newPassword)
        {
            var resetPasswordAsync = await base.ResetPasswordAsync(user, token, newPassword);
            if (resetPasswordAsync.Succeeded)
                systemLog.UserPasswordChanged(authorizedUser?.UserName, user.UserName);
            else
                systemLog.UserPasswordChangeFailed(authorizedUser?.UserName, user.UserName);
            return resetPasswordAsync;
        }
    }
}
