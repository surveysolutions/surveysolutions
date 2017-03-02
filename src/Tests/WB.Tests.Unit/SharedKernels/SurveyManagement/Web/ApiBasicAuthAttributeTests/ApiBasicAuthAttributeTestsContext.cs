using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class ApiBasicAuthAttributeTestsContext
    {
        protected static ApiBasicAuthAttribute CreateApiBasicAuthAttribute(Func<string, string, bool> isUserValid = null, IIdentityManager identityManager = null, IReadSideStatusService readSideStatusService = null)
        {
            Setup.InstanceToMockedServiceLocator(identityManager ?? Mock.Of<IIdentityManager>());
            Setup.InstanceToMockedServiceLocator(readSideStatusService ?? Mock.Of<IReadSideStatusService>());

            return new ApiBasicAuthAttribute(new [] {UserRoles.Interviewer});
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