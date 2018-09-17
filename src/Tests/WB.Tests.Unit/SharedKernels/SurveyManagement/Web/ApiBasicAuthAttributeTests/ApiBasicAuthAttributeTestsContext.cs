using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Code;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class ApiBasicAuthAttributeTestsContext
    {
        protected static ApiBasicAuthAttribute CreateApiBasicAuthAttribute(Func<string, string, bool> isUserValid = null, 
            IUserStore<HqUser, Guid> userStore = null)
        {
            var hqUserManager = Create.Storage.HqUserManager(userStore);
            var auth = new Mock<IAuthenticationManager>();
            var hqSignInManager = new HqSignInManager(hqUserManager, auth.Object, Mock.Of<IHashCompatibilityProvider>());
            Setup.InstanceToMockedServiceLocator(hqSignInManager);

            return new ApiBasicAuthAttribute(UserRoles.Interviewer);
        }

        protected static HttpActionContext CreateActionContext()
        {
            return
                new HttpActionContext(
                    new HttpControllerContext(new HttpRequestContext(), new HttpRequestMessage(new HttpMethod("POST"), new Uri("http://hq.org/api/sync")),
                        new HttpControllerDescriptor(), Mock.Of<IHttpController>()), new ReflectedHttpActionDescriptor());
        }
    }
}
