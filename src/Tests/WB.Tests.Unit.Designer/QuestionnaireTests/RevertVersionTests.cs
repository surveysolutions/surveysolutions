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
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    internal class RevertVersionTests : QuestionnaireTestsContext
    {
        [Test]
        public void RevertVersion_When_question_version_specified_Then_should_restore_questionnaire()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferenceId = Guid.NewGuid();
            var currentQuestionnaireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnaireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnaireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferenceId) == oldQuestionnaireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnaireDocument, null);
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferenceId, responsibleId);
            
            // Act
            questionnaire.RevertVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnaireDocument));
        }

        [Test]
        public void RevertVersion_When_shared_person_try_to_restore_questionnaire_Then_should_restore_questionnaire()
        {
            // Arrange
            Guid ownerId = Guid.NewGuid();
            Guid sharedPersonId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferenceId = Guid.NewGuid();
            var currentQuestionnaireDocument = CreateQuestionnaireDocument(createdBy: ownerId);
            var oldQuestionnaireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnaireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferenceId) == oldQuestionnaireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnaireDocument, new[] { new SharedPerson() { UserId = sharedPersonId} });
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferenceId, sharedPersonId);
            
            // Act
            questionnaire.RevertVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnaireDocument));
        }


        [Test]
        public void RevertVersion_When_person_without_permissions_do_revert_Then_should_throw_exception()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid personWhithNoPermissions = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferenceId = Guid.NewGuid();
            var currentQuestionnaireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnaireDocument = CreateQuestionnaireDocument();

            var historyVersionsService = Mock.Of<IQuestionnaireHistoryVersionsService>(s => s.GetByHistoryVersion(historyReferenceId) == oldQuestionnaireDocument);
            var questionnaire = Create.Questionnaire(historyVersionsService: historyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnaireDocument, null);
            var command = Create.Command.RevertVersionQuestionnaire(questionnaireId, historyReferenceId, personWhithNoPermissions);

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

        [Test]
        public void should_be_able_to_restore_questionnaire_from_only_diffs()
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
                diffWithPreviousVersion: patchGenerator.Diff(null, documentBeforeChange)), initialChangeId);

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
