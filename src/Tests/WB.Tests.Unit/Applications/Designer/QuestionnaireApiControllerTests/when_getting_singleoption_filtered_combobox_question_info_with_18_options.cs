using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_filtered_combobox_question_info_with_18_options : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var singleoptionFilteredCombobox = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 18, isFilteredCombobox: true);

            questionnaireInfoViewFactoryMock = new Mock<IQuestionnaireInfoFactory>();
            questionnaireInfoViewFactoryMock
                .Setup(x => x.GetQuestionEditView(questionnaireId, questionId))
                .Returns(singleoptionFilteredCombobox);
            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoViewFactoryMock.Object);
        };

        Because of = () =>
            result = controller.EditQuestion(questionnaireId, questionId);

        It should_return_edit_question_details = () =>
            result.ShouldNotBeNull();

        It should_return_edit_question_details_with_18_options = () =>
            result.Options.Length.ShouldEqual(18);

        It should_return_edit_question_details_with_WasOptionsTruncated_set_in_false = () =>
            result.WereOptionsTruncated.ShouldBeFalse();

        private static QuestionnaireController controller;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}