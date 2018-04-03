using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocumentFromRepository = Create.Entity.QuestionnaireDocument();
            questionnaireDocumentFromRepository.Title = originalTitle;

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem(questionnaireIdentity: questionnaireIdentity));

            plainQuestionnaireRepositoryMock = Mock.Get(Mock.Of<IQuestionnaireStorage>(_
                => _.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaireDocumentFromRepository));

            questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                questionnaireStorage: plainQuestionnaireRepositoryMock.Object);

            questionnaire.SetId(questionnaireIdentity.QuestionnaireId);
            BecauseOf();
        }

        public void BecauseOf() =>
            questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                questionnaireIdentity: questionnaireIdentity, newTitle: newQuestionnaireTitle,
                newQuestionnaireVersion: questionnaireIdentity.Version + 1));

        [NUnit.Framework.Test] public void should_not_store_questionnaire_document_which_was_read_from_repository () =>
            plainQuestionnaireRepositoryMock.Verify(
                repository => repository.StoreQuestionnaire(
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version + 1,
                    questionnaireDocumentFromRepository),
                Times.Never);

        [NUnit.Framework.Test] public void should_store_questionnaire_document_with_new_title () =>
            plainQuestionnaireRepositoryMock.Verify(
                repository => repository.StoreQuestionnaire(
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version + 1,
                    Moq.It.Is<QuestionnaireDocument>(document => document.Title == newQuestionnaireTitle)),
                Times.Once);

        [NUnit.Framework.Test] public void should_not_change_original_title () => 
            questionnaireDocumentFromRepository.Title.Should().Be(originalTitle);

        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
        private static string newQuestionnaireTitle = "New Questionnaire Title";
        private static Mock<IQuestionnaireStorage> plainQuestionnaireRepositoryMock;
        private static QuestionnaireDocument questionnaireDocumentFromRepository;
        private static readonly string originalTitle = "original title";
    }
}
