using System.Net;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorizing_and_request_header_has_bad_basic_auth_info : ApiBasicAuthAttributeTestsContext
    {
        Establish context = () =>
        {
            attribute = Create((userName, password)=> false);

            actionContext = CreateActionContext();
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "aaaaaaaa");
        };

        Because of = () => attribute.OnAuthorization(actionContext);

        It should_response_not_be_null = () =>
            actionContext.Response.ShouldNotBeNull();

        It should_be_unauthorized_response_status_code = () =>
            actionContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        It should_response_reason_phrase_contains_specified_message = () =>
            actionContext.Response.ReasonPhrase.ShouldEqual("Synchronization failed. User is not autorized. Please check you login/password for http://hq.org.");

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}