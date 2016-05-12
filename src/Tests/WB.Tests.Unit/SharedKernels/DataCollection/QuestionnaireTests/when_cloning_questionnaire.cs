using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: questionnaireIdentity.ToString(), entity: Create.QuestionnaireBrowseItem(questionnaireIdentity: questionnaireIdentity));

            plainQuestionnaireRepositoryMock = Mock.Get(Mock.Of<IPlainQuestionnaireRepository>(_
                => _.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaireDocumentFromRepository));

            questionnaire = Create.DataCollectionQuestionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object);

            questionnaire.SetId(questionnaireIdentity.QuestionnaireId);
        };

        Because of = () =>
            questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                questionnaireIdentity: questionnaireIdentity, newTitle: newQuestionnaireTitle));

        It should_store_questionnaire_document_which_was_read_from_repository = () =>
            plainQuestionnaireRepositoryMock.Verify(
                repository => repository.StoreQuestionnaire(
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version + 1,
                    questionnaireDocumentFromRepository),
                Times.Once);

        It should_store_questionnaire_document_with_new_title = () =>
            plainQuestionnaireRepositoryMock.Verify(
                repository => repository.StoreQuestionnaire(
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version + 1,
                    Moq.It.Is<QuestionnaireDocument>(document => document.Title == newQuestionnaireTitle)),
                Times.Once);

        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
        private static string newQuestionnaireTitle = "New Questionnaire Title";
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static QuestionnaireDocument questionnaireDocumentFromRepository = Create.QuestionnaireDocument();
    }
}