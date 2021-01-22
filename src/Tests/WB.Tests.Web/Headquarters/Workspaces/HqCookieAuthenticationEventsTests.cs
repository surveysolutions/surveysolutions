#nullable enable
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    [TestOf(typeof(HqCookieAuthenticationEvents))]
    public class HqCookieAuthenticationEventsTests
    {
        #pragma warning disable 8618 // fields are initialized in Setup()
        private IWorkspacesCache cache;

        private Mock<IUserRepository> userRepository;
        private Mock<IUserClaimsPrincipalFactory<HqUser>> hqUserPrincipalFactory;
        private Mock<IWorkspacesUsersCache> workspacesUsersCache;
        private HqCookieAuthenticationEvents Subject;
        #pragma warning restore 8618

        private readonly List<string> UserWorkspaces = new();

        [SetUp]
        public void Setup()
        {
            this.cache = Create.Service.WorkspacesCache(new []{ "primary" });
            this.userRepository = new Mock<IUserRepository>();
            this.hqUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<HqUser>>();
            this.workspacesUsersCache = new Mock<IWorkspacesUsersCache>();
            this.UserWorkspaces.Clear();
            this.workspacesUsersCache.Setup(w => w.GetUserWorkspaces(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(UserWorkspaces));

            this.Subject = new HqCookieAuthenticationEvents(cache,
                this.userRepository.Object,
                this.hqUserPrincipalFactory.Object,
                this.workspacesUsersCache.Object);
        }

        private ClaimsPrincipal GetUserPrincipal(Guid id, UserRoles[]? roles = null, params Claim[] claims)
        {
            IEnumerable<Claim> GetClaims()
            {
                yield return new Claim(ClaimTypes.NameIdentifier, id.ToString());

                foreach (var role in roles ?? new[] { UserRoles.Headquarter })
                {
                    yield return RoleClaim(role);
                }

                foreach (var claim in claims)
                {
                    yield return claim;
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(GetClaims()));
        }

        private CookieValidatePrincipalContext GetValidationContext(ClaimsPrincipal principal)
        {
            var httpContext = new DefaultHttpContext();

            var ctx = new CookieValidatePrincipalContext(httpContext,
                new AuthenticationScheme("Cookie",
                    null, typeof(BasicAuthenticationHandler)
                    ),
                new CookieAuthenticationOptions(),
                new AuthenticationTicket(principal, "Cookie"));

            this.userRepository.Setup(u => u.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    var user = new HqUser();
                    foreach (var principalClaim in principal.Claims)
                    {
                        user.Claims.Add(HqUserClaim.FromClaim(principalClaim));
                    }
                    return Task.FromResult(user);
                });

            return ctx;
        }

        Claim WorkspaceClaim(string name) => new(WorkspaceConstants.ClaimType, name);
        Claim WorkspaceRevisionClaim(int revision) => new(WorkspaceConstants.RevisionClaimType, revision.ToString());
        Claim ObserverClaim(string name) => new (AuthorizedUser.ObserverClaimType, name);
        Claim RoleClaim(UserRoles role) => new (ClaimTypes.Role, role.ToString());

        [Test]
        public async Task user_should_refresh_claims_if_workspaces_changed()
        {
            var user = GetUserPrincipal(Id.g1, new[] {UserRoles.Headquarter},
                WorkspaceRevisionClaim(0), // forcing refresh
                WorkspaceClaim("primary")
            );

            var ctx = GetValidationContext(user);

            await this.Subject.ValidatePrincipal(ctx);

            Assert.That(ctx.ShouldRenew, Is.EqualTo(true));
        }

        [Test]
        public async Task user_should_refresh_claims_when_own_workspaces_changed()
        {
            var revision = cache.Revision();
            var user = GetUserPrincipal(Id.g1, new[] { UserRoles.Headquarter },
                WorkspaceRevisionClaim(revision),
                WorkspaceClaim("primary"),
                WorkspaceClaim("test")
            );

            var ctx = GetValidationContext(user);

            UserWorkspaces.Add("test1");

            await this.Subject.ValidatePrincipal(ctx);

            Assert.That(ctx.ShouldRenew, Is.EqualTo(true));
        }        
        
        [Test]
        public async Task user_should_not_refresh_claims_when_no_workspaces_changed()
        {
            UserWorkspaces.Add("primary");
            UserWorkspaces.Add("test");

            var revision = cache.Revision();
            var user = GetUserPrincipal(Id.g1, new[] { UserRoles.Headquarter },
                WorkspaceRevisionClaim(revision),
                WorkspaceClaim("primary"),
                WorkspaceClaim("test")
            );

            var ctx = GetValidationContext(user);
            
            await this.Subject.ValidatePrincipal(ctx);

            Assert.That(ctx.ShouldRenew, Is.EqualTo(false));
        }

        [Test]
        public async Task user_should_preserve_observer_claims_when_claim_refreshed()
        {
            var user = GetUserPrincipal(Id.g1, new[] {UserRoles.Headquarter},
                WorkspaceRevisionClaim(0), // forcing refresh
                WorkspaceClaim("primary"),
                ObserverClaim("obs"), RoleClaim(UserRoles.Observer)
            );

            var ctx = GetValidationContext(user);

            await this.Subject.ValidatePrincipal(ctx);

            Assert.That(ctx.ShouldRenew, Is.EqualTo(true));
            
            hqUserPrincipalFactory.Verify(f => f
                .CreateAsync(
                    It.Is<HqUser>(u => 
                        HasClaim(u, ClaimTypes.Role, UserRoles.Observer.ToString())
                        && HasClaim(u, AuthorizedUser.ObserverClaimType, "obs")
                    )
                ), Times.Once
            );
        }

        bool HasClaim(HqUser user, string type, string value)
        {
            return user.Claims.Any(c => c.ClaimType == type && c.ClaimValue == value);
        }
    }
}
