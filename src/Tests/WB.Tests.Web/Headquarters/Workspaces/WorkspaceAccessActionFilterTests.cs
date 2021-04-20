using System;
using System.Collections.Generic;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    [TestOf(typeof(WorkspaceAccessActionFilter))]
    public class WorkspaceAccessActionFilterTests
    {
        private WorkspaceContext CurrentWorkspace { get; set; }
        private UserRoles? Role { get; set; }
        private string AuthenticationType { get; set; }
        private List<object> Attributes { get; set; }
        private List<string> UserWorkspaces { get; set; }
        private Mock<IAuthorizedUser> AuthorizedUser { get; set; }
        private Mock<IWorkspaceContextAccessor> ContextAccessor { get; set; }

        [SetUp]
        public void Setup()
        {
            AuthenticationType = "Cookie";

            Attributes = new List<object>
            {
                new AuthorizeAttribute()
            };

            Role = null;
            CurrentWorkspace = WorkspaceContext.Default;

            UserWorkspaces = new List<string>
            {
                CurrentWorkspace.Name
            };

            AuthorizedUser = new Mock<IAuthorizedUser>();
            AuthorizedUser.Setup(a => a.HasAccessToWorkspace(It.IsAny<string>()))
                .Returns<string>(s => UserWorkspaces.Contains(s));

            ContextAccessor = new Mock<IWorkspaceContextAccessor>();
            ContextAccessor.Setup(c => c.CurrentWorkspace())
                .Returns(() => CurrentWorkspace);
        }

        [Test]
        public void admin_should_allowed_access_to_admin_workspace()
        {
            CurrentWorkspace = Workspace.Admin.AsContext();
            Role = UserRoles.Administrator;

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void non_admin_should_not_be_allowed_access_to_special_workspace()
        {
            CurrentWorkspace = Workspace.Admin.AsContext();
            Role = UserRoles.Supervisor;

            // act
            var context = Act();

            // assert
            Assert.NotNull(context.Result);
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceAccessDisabledReason));
        }

        [Test]
        public void admin_should_not_be_allowed_to_disabled_workspace()
        {
            CurrentWorkspace = new WorkspaceContext("wspc", "Some", DateTime.Now);
            Role = UserRoles.Administrator;

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceDisabledReason));
        }

        [Test]
        public void authorized_user_with_access_to_workspace_allowed()
        {
            Role = UserRoles.Headquarter;

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void non_authorized_user_to_anonymous_method_allowed()
        {
            Role = UserRoles.Headquarter;
            Attributes.Clear();
            Attributes.Add(new AllowAnonymousAttribute());

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void authorized_user_with_no_access_to_workspace_forbidden()
        {
            Role = UserRoles.Headquarter;
            UserWorkspaces.Clear();
            UserWorkspaces.Add("other workspace");

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceAccessDisabledReason));
        }

        [Test]
        public void WebInterview_user_can_access_workspace()
        {
            Role = null;
            AuthenticationType = null;

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void should_be_able_to_call_api()
        {
            AuthenticationType = AuthType.TenantToken;
            Role = null;
            UserWorkspaces.Clear();

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void should_fallback_to_primary()
        {
            Role = UserRoles.Headquarter;
            CurrentWorkspace = null;

            Attributes.Add(new AllowPrimaryWorkspaceFallbackAttribute());
            UserWorkspaces.Clear();
            UserWorkspaces.Add(Workspace.Default.Name);

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void should_not_fallback_to_primary_if_user_has_no_access_to_primary()
        {
            Role = UserRoles.Headquarter;
            CurrentWorkspace = null;

            Attributes.Add(new AllowPrimaryWorkspaceFallbackAttribute());
            UserWorkspaces.Clear();

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceAccessDisabledReason));
        }

        [Test]
        public void should_not_be_able_to_call_api_disabled_workspace()
        {
            CurrentWorkspace = new WorkspaceContext("1", "1", DateTime.Now);
            AuthenticationType = AuthType.TenantToken;
            Role = null;
            UserWorkspaces.Clear();

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceDisabledReason));
        }

        [Test]
        public void should_allow_users_from_other_workspaces_if_ignore_attribute_present()
        {
            Role = UserRoles.Interviewer;

            Attributes.Add(new IgnoreWorkspacesLimitationAttribute());

            this.UserWorkspaces = new List<string> {"nonprimary"};

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void WebInterview_user_cannot_access_disabled_workspace()
        {
            Role = null;
            AuthenticationType = null;
            CurrentWorkspace = new WorkspaceContext("q", "q", DateTime.Now);

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceDisabledReason));
        }

        [Test]
        public void authenticated_request_to_nonAuthorized_allowed()
        {
            Role = UserRoles.Headquarter;

            Attributes = new()
            {
                new AllowAnonymousAttribute()
            };

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void authenticated_request_to_allowAnonymous_allowed()
        {
            Role = UserRoles.Headquarter;
            Attributes = new()
            {
                new AllowAnonymousAttribute()
            };

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }

        [Test]
        public void authenticated_request_to_with_no_allowAnonymous_and_no_authorization_to_disabled_workspace_notAllowed()
        {
            Role = UserRoles.Headquarter;
            CurrentWorkspace = new WorkspaceContext("q", "q", DateTime.Now);
            Attributes.Clear();

            // act
            var context = Act();

            // assert
            Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(GetForbidReason(context.Result), Is.EqualTo(ForbidReason.WorkspaceDisabledReason));
        }
        
        [Test]
        public void user_should_be_allowed_access_to_users_workspace()
        {
            CurrentWorkspace = Workspace.UsersWorkspace.AsContext();
            Role = UserRoles.Headquarter;

            // act
            var context = Act();

            // assert
            Assert.Null(context.Result);
        }


        private AuthorizationFilterContext Act()
        {
            var sut = CreateSut();
            var context = CreateContext();
            sut.OnAuthorization(context);
            return context;
        }

        private ForbidReason? GetForbidReason(IActionResult actionResult)
        {
            if (actionResult is ForbidResult result)
            {
                return result.Properties.TryGetForbidReason(out var reason) ? reason : null;
            }

            return null;
        }

        private AuthorizationFilterContext CreateContext()
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>();

            foreach (var workspace in UserWorkspaces)
            {
                claims.Add(new Claim(WorkspaceConstants.ClaimType, workspace));
            }

            if (Role != null) claims.Add(new Claim(ClaimTypes.Role, Role.Value.ToString()));

            var identity = new ClaimsIdentity(claims, AuthenticationType);
            var user = new ClaimsPrincipal(identity);
            httpContext.User = user;

            var actionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = Attributes
            };

            return new AuthorizationFilterContext(
                new ActionContext(httpContext, new RouteData(),
                    actionDescriptor), new List<IFilterMetadata>());
        }

        private WorkspaceAccessActionFilter CreateSut()
        {
            return new(ContextAccessor.Object, AuthorizedUser.Object);
        }


    }
}
