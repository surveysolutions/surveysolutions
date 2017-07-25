using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorizing_not_an_interviewer : ApiBasicAuthAttributeTestsContext
    {
        private Establish context = () =>
        {
            var mockOfUserManager = new Mock<IUserStore<HqUser, Guid>>();
            mockOfUserManager.Setup(_ => _.FindByNameAsync(Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(Create.Entity.HqUser(role: UserRoles.Headquarter, passwordHash: "open sesame")));

            attribute = CreateApiBasicAuthAttribute((userName, password) => true, mockOfUserManager.Object);

            actionContext = CreateActionContext();
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        };

        Because of = () => attribute.OnAuthorizationAsync(actionContext, CancellationToken.None).WaitAndUnwrapException();

        It should_be_unauthorized_response_status_code = () =>
            actionContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        It should_respond_with_user_friendly_ReasonPhrase = () =>
            new[] { "not", "role", "permitting" }.ShouldEachConformTo(keyword => actionContext.Response.ReasonPhrase.ToLower().Contains(keyword));

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}