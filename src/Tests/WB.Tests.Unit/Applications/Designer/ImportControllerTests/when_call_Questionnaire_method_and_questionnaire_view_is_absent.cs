using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_call_Questionnaire_method_and_questionnaire_view_is_absent : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = new DownloadQuestionnaireRequest() {QuestionnaireId = Guid.NewGuid()};
            importController = CreateImportController();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_NotFound = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            ((HttpResponseException) exception).Response.ReasonPhrase.ShouldEqual(
                string.Format(
                    "Questionnaire with id={0} cannot be found. Please check the list of available questionnaires, or contact the colleague who shared the questionnaire link with you.",
                    request.QuestionnaireId));

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
    }
}
