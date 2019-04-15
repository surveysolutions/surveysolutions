using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using WB.UI.Designer.Controllers.Api.Designer;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_filtered_combobox_question_info_with_more_than_200_options : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var singleoptionFilteredCombobox = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 250, isFilteredCombobox: true);

            questionnaireInfoViewFactoryMock = new Mock<IQuestionnaireInfoFactory>();
            questionnaireInfoViewFactoryMock
                .Setup(x => x.GetQuestionEditView(questionnaireId, questionId))
                .Returns(singleoptionFilteredCombobox);
            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoViewFactoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = (NewEditQuestionView) (controller.EditQuestion(questionnaireId, questionId) as OkObjectResult)?.Value;

        [NUnit.Framework.Test] public void should_return_edit_question_details () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_edit_question_details_with_200_options () =>
            result.Options.Length.Should().Be(200);

        [NUnit.Framework.Test] public void should_return_edit_question_details_with_WasOptionsTruncated_set_in_true () =>
            result.WereOptionsTruncated.Should().BeTrue();

        private static QuestionnaireApiController controller;
        private static string questionnaireId = "22222222222222222222222222222222";
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}
