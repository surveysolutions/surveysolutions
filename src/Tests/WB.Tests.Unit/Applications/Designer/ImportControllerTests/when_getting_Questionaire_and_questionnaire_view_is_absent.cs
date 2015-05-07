using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using Machine.Specifications.Utility;
using WB.UI.Designer.Api;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
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
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_NotFound = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            (new[] { "questionnaire", "cannot", "found", "shared"}).Each(x => ((HttpResponseException)exception).Response.ReasonPhrase.ToLower().ShouldContain(x));

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
    }
}
