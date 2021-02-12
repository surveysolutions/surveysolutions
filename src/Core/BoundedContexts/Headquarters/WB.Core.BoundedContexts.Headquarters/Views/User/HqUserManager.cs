#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserManager : UserManager<HqUser>
    {
        private readonly IWorkspacesService workspacesService;

        public HqUserManager(IUserStore<HqUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<HqUser> passwordHasher, 
            IEnumerable<IUserValidator<HqUser>> userValidators, 
            IEnumerable<IPasswordValidator<HqUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<HqUser>> logger,
            IWorkspacesService workspacesService) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.workspacesService = workspacesService;
        }

        public override async Task<IdentityResult> CreateAsync(HqUser user)
        {
            var result = await base.CreateAsync(user);

            if (result.Succeeded && !user.IsInRole(UserRoles.Administrator))
            {
                foreach (var userWorkspace in user.Workspaces)
                {
                    this.workspacesService.AddUserToWorkspace(user, userWorkspace.Workspace.Name);
                }
            }

            return result;
        }
    }
}
