using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewerQuestionnairesControllerTests
{
    public class when_getting_questionnaire_and_interviewer_content_version_is_less_then_in_questionnaire
    {
        Establish context = () =>
        {
            controller = Create.Controller.InterviewerQuestionnaires(questionnaire: new QuestionnaireDocument(), questionnaireBrowseItem:new QuestionnaireBrowseItem() {QuestionnaireContentVersion = HqQuestionnaireContentVersion });
        };

        Because of = () =>
            response = controller.Get(Id.g1, 1, InterviewerQuestionnaireContentVersion);
            
        It should_return_message_to_upgrade = () =>
            response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        private static HttpResponseMessage response;
        private static InterviewerQuestionnairesController controller;
        private static readonly int InterviewerQuestionnaireContentVersion = 7;
        private static readonly int HqQuestionnaireContentVersion = 12;
    }
}
