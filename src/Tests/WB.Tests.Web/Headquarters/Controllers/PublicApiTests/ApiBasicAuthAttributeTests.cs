using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using AutoFixture;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Tests.Abc;

using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class ApiBasicAuthAttributeTests
    {
        [Test]
        public void when_both_authorizations_supply_user_should_be_identified_by_cookie()
        {
            var subject = new ApiBasicAuthAttribute(UserRoles.ApiUser, UserRoles.Administrator)
            {
                FallbackToCookieAuth = true
            };

            var depScope = new Mock<IDependencyScope>();
            var actionContext = GetContext(depScope.Object);

            SetCookieAuthorization(actionContext, GetPrincipal("admin", UserRoles.Administrator));
            SetBasicAuthorization(actionContext, "api", "user");

            Assert.DoesNotThrowAsync(() => subject.OnAuthorizationAsync(actionContext, CancellationToken.None));

            depScope.Verify(d => d.GetService(It.IsAny<Type>()), Times.Never, 
                "Access to services is a mark that cookie auth not worked");
        }

        [Test]
        public void when_basic_auth_supplied_should_be_identified_by_cookie()
        {
            var subject = new ApiBasicAuthAttribute(UserRoles.ApiUser, UserRoles.Administrator)
            {
                FallbackToCookieAuth = true
            };
            var fixture = Create.Other.AutoFixture();


            var depScope = new Mock<IDependencyScope>();
            depScope.Setup(v => v.GetService(typeof(HqSignInManager)))
                .Returns(() => fixture.Create<HqSignInManager>());

            var actionContext = GetContext(depScope.Object);

            SetBasicAuthorization(actionContext, "api", "user");

            Assert.DoesNotThrowAsync(() => subject.OnAuthorizationAsync(actionContext, CancellationToken.None));

            depScope.Verify(d => d.GetService(It.IsAny<Type>()), Times.Once,
                "Access to services is a mark that cookie auth not worked");
        }

        HttpActionContext GetContext(IDependencyScope dependencyScope)
        {
            var dumbController = typeof(DumbController);

            var controllerDescriptor = new HttpControllerDescriptor(
                new HttpConfiguration(), "Test", typeof(DumbController));

            var controllerContext = new HttpControllerContext
            {
                ControllerDescriptor = controllerDescriptor,
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://example.com")
                }
            };

            var actionDescription = new ReflectedHttpActionDescriptor(controllerDescriptor, dumbController.GetMethod("Get"));

            var context = new HttpActionContext(controllerContext, actionDescription);
            context.Request.Properties.Add(HttpPropertyKeys.DependencyScope, dependencyScope);

            return context;
        }

        internal class DumbController : ApiController
        {
            public IHttpActionResult Get()
            {
                return Ok();
            }
        }

        void SetBasicAuthorization(HttpActionContext ctx, string name, string password)
        {
            ctx.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{name}:{password}")));

        }

        void SetCookieAuthorization(HttpActionContext ctx, IPrincipal principal)
        {
            ctx.ControllerContext.RequestContext.Principal = principal;
        }

        IPrincipal GetPrincipal(string name, UserRoles role, bool isAuthenticated = true)
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role.ToString()),
            }, "Cookie");

            var principal = new ClaimsPrincipal(claimsIdentity);

            return principal;
        }
    }
}
