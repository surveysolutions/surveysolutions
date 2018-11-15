using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    public abstract class ApiBasicAuthNunitBasedSpecification
    {
        [OneTimeSetUp]
        public async Task EstablishAsync()
        {
            this.Context();
            await this.BecauseAsync();
        }

        protected virtual void Context() { }

        protected virtual Task BecauseAsync() => Task.CompletedTask;

        protected ApiBasicAuthAttribute attribute;
        protected HttpActionContext actionContext;
        protected HqUserManager userManager;

        protected ApiBasicAuthAttribute CreateApiBasicAuthAttribute()
        {
            /*this.userManager = Create.Storage.HqUserManager(this.UserStore.Object, this.HashCompatibilityProvider.Object);
            var auth = new Mock<IAuthenticationManager>();
            var hashCompatibilityProvider = Mock.Of<IHashCompatibilityProvider>();
            var hqSignInManager = new HqSignInManager(userManager, auth.Object, hashCompatibilityProvider, this.ApiTokenProviderProvider.Object);
            Setup.InstanceToMockedServiceLocator(hqSignInManager);
            Setup.InstanceToMockedServiceLocator(this.HashCompatibilityProvider.Object);*/

            return new ApiBasicAuthAttribute(UserRoles.Interviewer);
        }

        protected void SetupInterviwer(HqUser hqUser)
        {
            this.UserStore.Setup(_ => _.FindByNameAsync(Moq.It.IsAny<string>())).Returns(Task.FromResult(hqUser));
            this.UserStore.Setup(_ => _.FindByIdAsync(Moq.It.IsAny<Guid>())).Returns(Task.FromResult(hqUser));
        }

        protected Mock<IUserStore<HqUser, Guid>> UserStore = new Mock<IUserStore<HqUser, Guid>>();
        protected Mock<IHashCompatibilityProvider> HashCompatibilityProvider = new Mock<IHashCompatibilityProvider>();
        protected Mock<IApiTokenProvider<Guid>> ApiTokenProviderProvider = new Mock<IApiTokenProvider<Guid>>();

        protected HttpActionContext CreateActionContext()
        {
            var context =
                new HttpActionContext(
                    new HttpControllerContext(
                        new HttpRequestContext(), 
                        new HttpRequestMessage(new HttpMethod("POST"), new Uri("http://hq.org/api/sync")),
                        new HttpControllerDescriptor(), Mock.Of<IHttpController>()), 
                    new ReflectedHttpActionDescriptor());

            
            userManager = Create.Storage.HqUserManager(this.UserStore.Object, this.HashCompatibilityProvider.Object);
            var auth = new Mock<IAuthenticationManager>();

            //var hashCompatibilityProvider = Mock.Of<IHashCompatibilityProvider>();
            var hqSignInManager = new HqSignInManager(userManager, auth.Object, this.HashCompatibilityProvider.Object, this.ApiTokenProviderProvider.Object);

            var scopeMock = new Mock<IDependencyScope>();
            scopeMock.Setup(x => x.GetService(typeof(HqSignInManager))).Returns(hqSignInManager);
            context.Request.Properties.Add(HttpPropertyKeys.DependencyScope, scopeMock.Object);

            return context;
        }
    }
}
