using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class RevertVersionTests : QuestionnaireTestsContext
    {
        [Test]
        public void RevertVersion_When_question_version_specified_Then_should_restore_questionnire()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, null);
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferanceId, responsibleId);
            
            // Act
            questionnaire.RevertVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnireDocument));
        }

        [Test]
        public void RevertVersion_When_shared_person_try_to_restore_questionnire_Then_should_restore_questionnire()
        {
            // Arrange
            Guid ownerId = Guid.NewGuid();
            Guid sharedPersonId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: ownerId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, new[] { new SharedPerson() { UserId = sharedPersonId} });
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferanceId, sharedPersonId);
            
            // Act
            questionnaire.RevertVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnireDocument));
        }


        [Test]
        public void RevertVersion_When_person_without_permissions_do_revert_Then_should_throw_exception()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid personWhitoutPermissions = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, null);
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferanceId, personWhitoutPermissions);

            // Act
            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.RevertVersion(command));

            // Assert
            Assert.That(exception.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        public void when_there_is_diff_and_full_copy_of_document_present_in_history_Should_use_snapshot_to_restore()
        {
            var serializer =  new EntitySerializer<QuestionnaireDocument>();
            IPlainStorageAccessor<QuestionnaireChangeRecord> historyStorage = new Abc.Storage.TestPlainStorage<QuestionnaireChangeRecord>();
            var patchGenerator = Create.PatchGenerator();

            var questionId = Id.g1;
            var document = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId: questionId, text: "not edited text")
            });

            var initialChangeId = Id.gA.FormatGuid();
            var documentBeforeChange = serializer.Serialize(document);
            historyStorage.Store(Create.QuestionnaireChangeRecord(questionnaireChangeRecordId: initialChangeId,
                sequence: 1,
                resultingQuestionnaireDocument: documentBeforeChange), initialChangeId);

            document.UpdateQuestion(questionId, q => q.QuestionText = "edited text");
            var documentAfterChange = serializer.Serialize(document);

            var secondChangeId = Id.gB.FormatGuid();
            historyStorage.Store(Create.QuestionnaireChangeRecord(questionnaireChangeRecordId: secondChangeId,
                sequence: 2,
                diffWithPreviousVersion: patchGenerator.Diff(documentBeforeChange, documentAfterChange)), secondChangeId);

            var service = Create.QuestionnireHistoryVersionsService(historyStorage);

            // Act
            QuestionnaireDocument patchedDocument = service.GetByHistoryVersion(Guid.Parse(secondChangeId));

            // Assert
            Assert.That(patchedDocument.GetQuestion<TextQuestion>(questionId).QuestionText, Is.EqualTo("edited text"));
        }
    }
}
