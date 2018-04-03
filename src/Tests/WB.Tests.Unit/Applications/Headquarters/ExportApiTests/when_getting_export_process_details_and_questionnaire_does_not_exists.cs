using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_process_details_and_questionnaire_does_not_exists : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var mockOfQuestionnaireBrowseViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            mockOfQuestionnaireBrowseViewFactory.Setup(x => x.GetById(questionnaireIdentity)).Returns((QuestionnaireBrowseItem)null);

            controller = CreateExportController(questionnaireBrowseViewFactory: mockOfQuestionnaireBrowseViewFactory.Object);
            BecauseOf();
        }

        private void BecauseOf() => result = controller.ProcessDetails(questionnaireIdentity.ToString(), DataExportFormat.Tabular);

        [NUnit.Framework.Test] public void should_return_http_not_found_response () =>
            ((NegotiatedContentResult<string>)result).StatusCode.Should().Be(HttpStatusCode.NotFound);

        [NUnit.Framework.Test] public void should_response_has_specified_message () =>
            ((NegotiatedContentResult<string>)result).Content.Should().Be("Questionnaire not found");

        private static ExportController controller;
        private static IHttpActionResult result;

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}
