#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class HqJwtAuthenticationEvents : JwtBearerEvents  
    {
        private readonly IInScopeExecutor<IUserRepository> userRepository;
        private readonly IUserClaimsPrincipalFactory<HqUser> claimFactory;
        private bool isUserLocked;
        private const string FailureMessage = "Invalid user";

        public HqJwtAuthenticationEvents(IInScopeExecutor<IUserRepository> userRepository, IUserClaimsPrincipalFactory<HqUser> claimFactory)
        {
            this.userRepository = userRepository;
            this.claimFactory = claimFactory;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var userPrincipal = context.Principal;
            var userIdentity = userPrincipal?.Identity as ClaimsIdentity;

            if (userIdentity == null)
            {
                context.Fail(FailureMessage);
                return;
            }

            try
            {
                Claim? GetFirstClaim(string type) => userIdentity.Claims.FirstOrDefault(c => c.Type == type);
                var id = GetFirstClaim(ClaimTypes.NameIdentifier)?.Value;

                if (id == null || !Guid.TryParse(id, out var userId))
                {
                    context.Fail(FailureMessage);
                    return;
                }
            
                await this.userRepository.ExecuteAsync(async u =>
                {
                    var hqUser = await u.FindByIdAsync(userId);
                
                    if (hqUser.IsArchivedOrLocked)
                    {
                        this.isUserLocked = true;
                        context.Fail("User is locked");
                        return Task.CompletedTask;
                    }
                    
                    var observerClaim = GetFirstClaim(AuthorizedUser.ObserverClaimType);

                    if (observerClaim != null)
                    {
                        hqUser.Claims.Add(HqUserClaim.FromClaim(observerClaim));

                        hqUser.Claims.Add(new HqUserClaim
                        {
                            ClaimType = ClaimTypes.Role,
                            ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
                        });

                        foreach (var claimedWorkspace in userIdentity.Claims.Where(c => c.Type == WorkspaceConstants.ClaimType))
                            hqUser.Claims.Add(HqUserClaim.FromClaim(claimedWorkspace));
                    }
                    context.Principal = await claimFactory.CreateAsync(hqUser);
                    return Task.CompletedTask;
                });

            }
            catch (Exception e)
            {
                context.Fail(FailureMessage);
            }
            
        }
    }
}
