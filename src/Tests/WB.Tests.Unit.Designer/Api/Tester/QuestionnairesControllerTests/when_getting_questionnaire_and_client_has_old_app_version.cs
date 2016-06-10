using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.QuestionnairesControllerTests
{
    public class when_getting_questionnaire_and_client_has_old_app_version : QuestionnairesControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnairesController();
        };

        Because of = () => expectedException =
            Catch.Exception(() => controller.Get(version: ApiVersion.Tester - 1, id: Guid.Parse("11111111111111111111111111111111")));

        It should_response_code_be_UpgradeRequired = () =>
            ((HttpResponseException)expectedException).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        private static Exception expectedException;
        private static QuestionnairesController controller;
    }
}