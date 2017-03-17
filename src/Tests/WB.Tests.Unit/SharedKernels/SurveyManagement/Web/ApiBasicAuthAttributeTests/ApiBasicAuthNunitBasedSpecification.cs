using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Moq;
using Nito.AsyncEx.Synchronous;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    public abstract class ApiBasicAuthNunitBasedSpecification
    {
        [OneTimeSetUp]
        public void Establish()
        {
            this.Context();
            this.Because();
        }

        protected virtual void Context()
        {

        }

        protected virtual void Because() => this.attribute.OnAuthorizationAsync(this.actionContext, CancellationToken.None).WaitAndUnwrapException();

        protected ApiBasicAuthAttribute attribute;
        protected HttpActionContext actionContext;
        protected HqUserManager userManager;
        protected ApiBasicAuthAttribute CreateApiBasicAuthAttribute(IReadSideStatusService readSideStatusService = null)
        {
            this.userManager = new HqUserManager(this.UserStore.Object, Mock.Of<IAuthorizedUser>(), this.HashCompatibilityProvider.Object, this.ApiTokenProviderProvider.Object);

            Setup.InstanceToMockedServiceLocator(this.userManager);
            Setup.InstanceToMockedServiceLocator(readSideStatusService ?? Mock.Of<IReadSideStatusService>());
            Setup.InstanceToMockedServiceLocator(this.HashCompatibilityProvider.Object);

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

        protected static HttpActionContext CreateActionContext()
        {
            return
                new HttpActionContext(
                    new HttpControllerContext(new HttpRequestContext(), 
                        new HttpRequestMessage(new HttpMethod("POST"), new Uri("http://hq.org/api/sync")),
                        new HttpControllerDescriptor(), Mock.Of<IHttpController>()), 
                    new ReflectedHttpActionDescriptor());
        }
    }
}