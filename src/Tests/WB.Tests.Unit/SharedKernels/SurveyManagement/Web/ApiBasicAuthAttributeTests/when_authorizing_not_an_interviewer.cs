using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorizing_not_an_interviewer : ApiBasicAuthAttributeTestsContext
    {
        private Establish context = () =>
        {
            var mockOfIdentityManager = new Mock<IIdentityManager>();
            mockOfIdentityManager.Setup(_ => _.IsUserValidWithPasswordHash(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(true);
            mockOfIdentityManager.Setup(_ => _.GetUserByName(Moq.It.IsAny<string>()))
                .Returns(Create.Entity.ApplicationUser(role: UserRoles.Headquarter));

            attribute = CreateApiBasicAuthAttribute((userName, password) => true, mockOfIdentityManager.Object);

            actionContext = CreateActionContext();
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        };

        Because of = () => attribute.OnAuthorization(actionContext);

        It should_be_unauthorized_response_status_code = () =>
            actionContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        It should_respond_with_user_friendly_ReasonPhrase = () =>
            new[] { "not", "role", "permitting" }.ShouldEachConformTo(keyword => actionContext.Response.ReasonPhrase.ToLower().Contains(keyword));

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}