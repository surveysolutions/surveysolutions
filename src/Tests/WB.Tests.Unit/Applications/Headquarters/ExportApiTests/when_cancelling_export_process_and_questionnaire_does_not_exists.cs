using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    extern alias datacollection;

    public class when_cancelling_export_process_and_questionnaire_does_not_exists : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            var mockOfQuestionnaireBrowseViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            mockOfQuestionnaireBrowseViewFactory.Setup(x => x.GetById(questionnaireIdentity)).Returns((QuestionnaireBrowseItem)null);

            controller = CreateExportController(questionnaireBrowseViewFactory: mockOfQuestionnaireBrowseViewFactory.Object);
        };

        Because of = () => result = controller.CancelProcess(questionnaireIdentity.ToString(), DataExportFormat.Tabular);

        It should_return_http_not_found_response = () =>
            ((NegotiatedContentResult<string>)result).StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_response_has_specified_message = () =>
            ((NegotiatedContentResult<string>)result).Content.ShouldEqual("Questionnaire not found");

        private static ExportController controller;
        private static IHttpActionResult result;

        private static readonly datacollection::WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity questionnaireIdentity = new datacollection::WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}