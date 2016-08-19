using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_starting_export_process_and_questionnaire_does_not_exists : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            var mockOfQuestionnaireBrowseViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            mockOfQuestionnaireBrowseViewFactory.Setup(x => x.GetById(questionnaireIdentity)).Returns((QuestionnaireBrowseItem)null);

            controller = CreateExportController(questionnaireBrowseViewFactory: mockOfQuestionnaireBrowseViewFactory.Object);
        };

        Because of = () => result = controller.StartProcess(questionnaireIdentity.ToString(), "tabular");

        It should_return_http_not_found_response = () =>
            ((NegotiatedContentResult<string>)result).StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_response_has_specified_message = () =>
            ((NegotiatedContentResult<string>)result).Content.ShouldEqual("Questionnaire not found");

        private static ExportController controller;
        private static IHttpActionResult result;

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}