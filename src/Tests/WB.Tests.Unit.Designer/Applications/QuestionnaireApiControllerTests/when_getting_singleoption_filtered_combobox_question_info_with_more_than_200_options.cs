using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_filtered_combobox_question_info_with_more_than_200_options : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var singleoptionFilteredCombobox = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 250, isFilteredCombobox: true);

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

        It should_return_edit_question_details_with_200_options = () =>
            result.Options.Length.ShouldEqual(200);

        It should_return_edit_question_details_with_WasOptionsTruncated_set_in_true = () =>
            result.WereOptionsTruncated.ShouldBeTrue();

        private static QuestionnaireController controller;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}