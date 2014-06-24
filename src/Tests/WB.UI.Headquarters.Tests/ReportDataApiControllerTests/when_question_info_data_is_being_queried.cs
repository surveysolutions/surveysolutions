using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ReportDataApiControllerTests
{
    internal class when_question_info_data_is_being_queried : ReportDataApiControllerTestContext
    {
        Establish context = () =>
        {
            questionInforFactory = new Mock<IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>>();
            questionInforFactory.Setup(x => x.Load(input)).Returns(view);
            controller = CreateReportDataApiController(questionInforFactory: questionInforFactory.Object);
        };

        Because of = () =>
            resultView = controller.QuestionInfo(input);

        It should_load_data_from_factory_once = () =>
            questionInforFactory.Verify(x => x.Load(input), Times.Once());

        It should_return_same_view_as_was_setted_up = () =>
            resultView.ShouldBeTheSameAs(view);

        private static QuestionnaireQuestionInfoInputModel input;
        private static QuestionnaireQuestionInfoView view;
        private static QuestionnaireQuestionInfoView resultView;
        private static ReportDataApiController controller;
        private static Mock<IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>> questionInforFactory;

    }
}