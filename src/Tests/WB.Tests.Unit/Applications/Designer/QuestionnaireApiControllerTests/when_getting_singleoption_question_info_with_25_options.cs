using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_question_info_with_25_options : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var singleoption = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 25, isFilteredCombobox: false);

            questionnaireInfoViewFactoryMock = new Mock<IQuestionnaireInfoFactory>();
            questionnaireInfoViewFactoryMock
                .Setup(x => x.GetQuestionEditView(questionnaireId, questionId))
                .Returns(singleoption);
            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoViewFactoryMock.Object);
        };

        Because of = () =>
            result = controller.EditQuestion(questionnaireId, questionId);

        It should_return_edit_question_details = () =>
            result.ShouldNotBeNull();

        It should_return_edit_question_details_with_25_options = () =>
            result.Options.Length.ShouldEqual(25);

        private static QuestionnaireController controller;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}