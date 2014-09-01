using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ReportDataApiControllerTests
{
    internal class when_questionnaires_data_is_being_queried : ReportDataApiControllerTestContext
    {
        Establish context = () =>
        {
            questionnaireBrowseViewFactory = new Mock<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView>>();
            questionnaireBrowseViewFactory.Setup(x => x.Load(input)).Returns(view);
            controller = CreateReportDataApiController(questionnaireBrowseViewFactory: questionnaireBrowseViewFactory.Object);
        };

        Because of = () =>
            resultView = controller.Questionnaires(input);

        It should_load_data_from_factory_once = () =>
            questionnaireBrowseViewFactory.Verify(x => x.Load(input), Times.Once());

        It should_return_same_view_as_was_setted_up = () => 
            resultView.ShouldBeTheSameAs(view);

        private static QuestionnaireBrowseInputModel input;
        private static QuestionnaireAndVersionsView view;
        private static QuestionnaireAndVersionsView resultView;
        private static ReportDataApiController controller;
        private static Mock<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView>> questionnaireBrowseViewFactory;

    }
}