using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view : QuestionnaireInfoViewFactoryContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var repositoryMock = new Mock<IDesignerQuestionnaireStorage>();

            repositoryMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(CreateQuestionnaireDocument(questionnaireId.QuestionnaireId.FormatGuid(), questionnaireTitle));

            var dbContext = Create.InMemoryDbContext();
            dbContext.Questionnaires.Add(Create.QuestionnaireListViewItem(id: questionnaireId.QuestionnaireId));
            dbContext.SaveChanges();

            factory = CreateQuestionnaireInfoViewFactory(dbContext: dbContext, repository: repositoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            view = factory.Load(questionnaireId, userId);

        [NUnit.Framework.Test]
        public void should_find_questionnaire() =>
            view.Should().NotBeNull();

        [NUnit.Framework.Test]
        public void should_questionnaire_id_be_equal_questionnaireId() =>
            view.QuestionnaireId.Should().Be(questionnaireId.QuestionnaireId.FormatGuid());

        [NUnit.Framework.Test]
        public void should_questionnaire_title_be_equal_questionnaireTitle() =>
            view.Title.Should().Be(questionnaireTitle);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
    }
}
