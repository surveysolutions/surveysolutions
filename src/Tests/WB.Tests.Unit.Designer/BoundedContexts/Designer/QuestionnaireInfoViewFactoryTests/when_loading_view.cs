using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view : QuestionnaireInfoViewFactoryContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var repositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(CreateQuestionnaireDocument(questionnaireId, questionnaireTitle));

            factory = CreateQuestionnaireInfoViewFactory(repository: repositoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            view = factory.Load(questionnaireId, userId);

        [NUnit.Framework.Test] public void should_find_questionnaire () =>
            view.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_questionnaire_id_be_equal_questionnaireId () =>
            view.QuestionnaireId.Should().Be(questionnaireId);

        [NUnit.Framework.Test] public void should_questionnaire_title_be_equal_questionnaireTitle () =>
            view.Title.Should().Be(questionnaireTitle);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
    }
}