using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_questionnaires_controller_questionnaries_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            questionnaireBrowseViewFactoryMock = new
                Mock<IQuestionnaireBrowseViewFactory>();
            controller = CreateQuestionnairesController(questionnaireBrowseViewViewFactory: questionnaireBrowseViewFactoryMock.Object);
        };
        
        Because of = () =>
        {
            actionResult = controller.Questionnaires(10, 1);
        };

        It should_return_QuestionnaireApiView = () =>
            actionResult.ShouldBeOfExactType<QuestionnaireApiView>();

        It should_call_factory_load_once = () =>
            questionnaireBrowseViewFactoryMock.Verify(x => x.Load(Moq.It.IsAny<QuestionnaireBrowseInputModel>()), Times.Once());

        private static QuestionnaireApiView actionResult;
        private static QuestionnairesController controller;
        private static Mock<IQuestionnaireBrowseViewFactory> questionnaireBrowseViewFactoryMock;
    }
}
