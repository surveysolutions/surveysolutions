using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorizing_not_an_interviewer : ApiBasicAuthAttributeTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var mockOfUserManager = new Mock<IUserStore<HqUser, Guid>>();
            mockOfUserManager.Setup(_ => _.FindByNameAsync(Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(Create.Entity.HqUser(role: UserRoles.Headquarter, passwordHash: "open sesame")));

            attribute = CreateApiBasicAuthAttribute();

            actionContext = CreateActionContext((userName, password) => true, mockOfUserManager.Object);
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
            BecauseOf();
        }

        public void BecauseOf() => attribute.OnAuthorizationAsync(actionContext, CancellationToken.None).WaitAndUnwrapException();

        [NUnit.Framework.Test] public void should_be_unauthorized_response_status_code () =>
            actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        [NUnit.Framework.Test] public void should_respond_with_user_friendly_ReasonPhrase () =>
            new[] { "not", "role", "permitting" }.Should().OnlyContain(keyword => actionContext.Response.ReasonPhrase.ToLower().Contains(keyword));

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}
