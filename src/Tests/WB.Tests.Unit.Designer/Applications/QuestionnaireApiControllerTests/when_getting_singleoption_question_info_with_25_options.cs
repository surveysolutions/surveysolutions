using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using WB.UI.Designer.Controllers.Api.Designer;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_question_info_with_25_options : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var singleoption = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 25, isFilteredCombobox: false);

            questionnaireInfoViewFactoryMock = new Mock<IQuestionnaireInfoFactory>();
            questionnaireInfoViewFactoryMock
                .Setup(x => x.GetQuestionEditView(questionnaireId, questionId))
                .Returns(singleoption);
            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoViewFactoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = (NewEditQuestionView) (controller.EditQuestion(questionnaireId, questionId) as OkObjectResult)?.Value;

        [NUnit.Framework.Test] public void should_return_edit_question_details () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_edit_question_details_with_25_options () =>
            result.Options.Length.Should().Be(25);

        private static QuestionnaireApiController controller;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}
