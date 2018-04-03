using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http.Controllers;
using FluentAssertions;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.UI.Headquarters.Code;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorizing_non_existant_user : ApiBasicAuthAttributeTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attribute = CreateApiBasicAuthAttribute((userName, password)=> false);

            actionContext = CreateActionContext();
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
            BecauseOf();
        }

        public void BecauseOf() => attribute.OnAuthorizationAsync(actionContext, CancellationToken.None).WaitAndUnwrapException();

        [NUnit.Framework.Test] public void should_response_not_be_null () =>
            actionContext.Response.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_be_unauthorized_response_status_code () =>
            actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        [NUnit.Framework.Test] public void should_response_reason_phrase_contains_specified_message () =>
           actionContext.Response.ReasonPhrase.Should().Be("Synchronization failed. User is not authorized. Please check your login/password for http://hq.org.");

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}
