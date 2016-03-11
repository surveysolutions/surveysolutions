using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_questionnaire_view_is_absent : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(Guid.NewGuid());
            importController = CreateImportController();
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldNotBeNull();

        It should_throw_HttpResponseException_with_StatusCode_NotFound = () =>
            exception.Response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().ShouldContain("questionnaire", "cannot", "found", "shared");

        private static ImportV2Controller importController;
        private static HttpResponseException exception;
        private static DownloadQuestionnaireRequest request;
    }
}
