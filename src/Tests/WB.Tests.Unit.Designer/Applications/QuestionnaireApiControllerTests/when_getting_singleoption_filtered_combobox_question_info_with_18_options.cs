using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers.Api.Designer;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_singleoption_filtered_combobox_question_info_with_18_options 
        : QuestionnaireApiControllerTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var singleoptionFilteredCombobox = CreateSingleoptionFilteredCombobox(questionId, optionsCount: 18, isFilteredCombobox: true);

            questionnaireInfoViewFactoryMock = new Mock<IQuestionnaireInfoFactory>();
            questionnaireInfoViewFactoryMock
                .Setup(x => x.GetQuestionEditView(questionnaireId, questionId))
                .Returns(singleoptionFilteredCombobox);
            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoViewFactoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = (controller.EditQuestion(questionnaireId, questionId) as OkObjectResult).Value as NewEditQuestionView;

        [Test]
        public void should_return_edit_question_details() =>
            result.Should().NotBeNull();

        [Test]
        public void should_return_edit_question_details_with_18_options() =>
            result.Options.Length.Should().Be(18);

        [Test]
        public void should_return_edit_question_details_with_WasOptionsTruncated_set_in_false() =>
            result.WereOptionsTruncated.Should().BeFalse();

        private static QuestionnaireApiController controller;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IQuestionnaireInfoFactory> questionnaireInfoViewFactoryMock;
        private static NewEditQuestionView result;
    }
}
