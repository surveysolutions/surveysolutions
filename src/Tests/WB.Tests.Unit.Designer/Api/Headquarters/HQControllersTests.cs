#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers.Api.Headquarters;
using WB.UI.Designer.Models;
using Main.Core.Documents;

namespace WB.Tests.Unit.Designer.Api.Headquarters
{
    internal static class HqTestHelpers
    {
        public static ClaimsPrincipal Principal(Guid? id = null, params string[] roles)
        {
            var identity = new ClaimsIdentity("test");
            if (id.HasValue)
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
            foreach (var role in roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            return new ClaimsPrincipal(identity);
        }
    }

    [TestFixture]
    [TestOf(typeof(HQControllerBase))]
    public class HQControllerBaseTests
    {
        private class TestableController : HQControllerBase
        {
            public TestableController(ClaimsPrincipal user)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            }

            public bool Access(QuestionnaireView view) => ValidateAccessPermissions(view);
            public bool AccessOrAdmin(QuestionnaireView view) => ValidateAccessPermissionsOrAdmin(view);
        }

        private static QuestionnaireView View(Guid? createdBy, params Guid[] sharedWith)
            => new QuestionnaireView(
                new QuestionnaireDocument { CreatedBy = createdBy },
                sharedWith.Select(x => new SharedPersonView { UserId = x }));

        [Test]
        public void should_allow_access_to_owner()
        {
            var userId = Guid.NewGuid();
            var controller = new TestableController(HqTestHelpers.Principal(userId));

            controller.Access(View(userId)).Should().BeTrue();
        }

        [Test]
        public void should_allow_access_to_shared_person()
        {
            var userId = Guid.NewGuid();
            var controller = new TestableController(HqTestHelpers.Principal(userId));

            controller.Access(View(Guid.NewGuid(), Guid.NewGuid(), userId)).Should().BeTrue();
        }

        [Test]
        public void should_deny_access_to_unrelated_user()
        {
            var controller = new TestableController(HqTestHelpers.Principal(Guid.NewGuid()));

            controller.Access(View(Guid.NewGuid(), Guid.NewGuid())).Should().BeFalse();
        }

        [Test]
        public void should_deny_access_when_questionnaire_has_no_creator_and_no_shares()
        {
            var controller = new TestableController(HqTestHelpers.Principal(Guid.NewGuid()));

            controller.Access(View(null)).Should().BeFalse();
        }

        [Test]
        public void should_throw_when_user_has_no_identifier_claim()
        {
            var controller = new TestableController(HqTestHelpers.Principal(null));

            Assert.Throws<InvalidOperationException>(() => controller.Access(View(Guid.NewGuid())));
        }

        [Test]
        public void should_not_let_admin_through_plain_access_check()
        {
            var controller = new TestableController(HqTestHelpers.Principal(Guid.NewGuid(), "Administrator"));

            controller.Access(View(Guid.NewGuid())).Should().BeFalse();
        }

        [Test]
        public void should_allow_admin_on_or_admin_check()
        {
            var controller = new TestableController(HqTestHelpers.Principal(Guid.NewGuid(), "Administrator"));

            controller.AccessOrAdmin(View(Guid.NewGuid())).Should().BeTrue();
        }

        [Test]
        public void should_allow_owner_on_or_admin_check_without_admin_role()
        {
            var userId = Guid.NewGuid();
            var controller = new TestableController(HqTestHelpers.Principal(userId));

            controller.AccessOrAdmin(View(userId)).Should().BeTrue();
        }

        [Test]
        public void should_deny_non_admin_unrelated_user_on_or_admin_check()
        {
            var controller = new TestableController(HqTestHelpers.Principal(Guid.NewGuid(), "User"));

            controller.AccessOrAdmin(View(Guid.NewGuid(), Guid.NewGuid())).Should().BeFalse();
        }
    }

    [TestFixture]
    [TestOf(typeof(HQUserController))]
    public class HQUserControllerTests
    {
        private static HQUserController Create(ClaimsPrincipal? user)
        {
            var controller = new HQUserController();
            if (user != null)
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            return controller;
        }

        [Test]
        public void should_require_authorization()
        {
            typeof(HQUserController).GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
        }

        [Test]
        public void should_expose_expected_routes()
        {
            typeof(HQUserController).GetCustomAttribute<RouteAttribute>()!.Template.Should().Be("api/hq/user");
            typeof(HQUserController).GetMethod(nameof(HQUserController.Login))!
                .GetCustomAttribute<RouteAttribute>()!.Template.Should().Be("login");
            typeof(HQUserController).GetMethod(nameof(HQUserController.UserDetails))!
                .GetCustomAttribute<RouteAttribute>()!.Template.Should().Be("userdetails");
        }

        [Test]
        public void login_should_not_throw()
        {
            Assert.DoesNotThrow(() => Create(HqTestHelpers.Principal(Guid.NewGuid())).Login());
        }

        [Test]
        public void user_details_should_return_null_when_there_is_no_user()
        {
            Create(null).UserDetails().Should().BeNull();
        }

        [Test]
        public void user_details_should_return_user_model()
        {
            var id = Guid.NewGuid();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, "login"),
                new Claim(ClaimTypes.Email, "u@example.org"),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "User"),
            }, "test");

            var result = Create(new ClaimsPrincipal(identity)).UserDetails();

            var model = result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<PortalUserModel>().Subject;
            model.Id.Should().Be(id);
            model.Login.Should().Be("login");
            model.Email.Should().Be("u@example.org");
            model.Roles.Should().BeEquivalentTo("Administrator", "User");
        }

        [Test]
        public void user_details_should_use_last_name_claim_as_full_name()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "login"),
                new Claim(ClaimTypes.Name, "Full Name"),
            }, "test");

            var result = Create(new ClaimsPrincipal(identity)).UserDetails();

            var model = (PortalUserModel)((OkObjectResult)result!).Value!;
            model.Login.Should().Be("login");
            model.FullName.Should().Be("Full Name");
        }

        [Test]
        public void user_details_should_return_empty_roles_and_null_email_when_claims_absent()
        {
            var result = Create(HqTestHelpers.Principal(Guid.NewGuid())).UserDetails();

            var model = (PortalUserModel)((OkObjectResult)result!).Value!;
            model.Roles.Should().BeEmpty();
            model.Email.Should().BeNull();
            model.FullName.Should().BeNull();
        }

        [Test]
        public void user_details_should_throw_when_user_id_claim_is_missing()
        {
            Assert.Throws<InvalidOperationException>(() => Create(HqTestHelpers.Principal(null)).UserDetails());
        }
    }
}


