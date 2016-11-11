using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class RestoreVersionTests : QuestionnaireTestsContext
    {
        [Test]
        public void RestoreVersion_When_question_version_specified_Then_should_restore_questionnire()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var histotyVersionsService = Mock.Of<IQuestionnireHistotyVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(histotyVersionsService: histotyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, null);
            var command = Create.Command.RestoreVersionQuestionnaire(questionnaireId, historyReferanceId, responsibleId);
            
            // Act
            questionnaire.RestoreVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnireDocument));
        }

        [Test]
        public void RestoreVersion_When_shared_person_try_to_restore_questionnire_Then_should_restore_questionnire()
        {
            // Arrange
            Guid ownerId = Guid.NewGuid();
            Guid sharedPersonId = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: ownerId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var histotyVersionsService = Mock.Of<IQuestionnireHistotyVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(histotyVersionsService: histotyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, new[] { new SharedPerson() { Id = sharedPersonId} });
            var command = Create.Command.RestoreVersionQuestionnaire(questionnaireId, historyReferanceId, sharedPersonId);
            
            // Act
            questionnaire.RestoreVersion(command);

            // Assert
            Assert.That(questionnaire.QuestionnaireDocument, Is.EqualTo(oldQuestionnireDocument));
        }


        [Test]
        public void RestoreVersion_When_person_without_permissions_do_revert_Then_should_throw_exception()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Guid personWhitoutPermissions = Guid.NewGuid();
            Guid questionnaireId = Guid.NewGuid();
            Guid historyReferanceId = Guid.NewGuid();
            var currentQuestionnireDocument = CreateQuestionnaireDocument(createdBy: responsibleId);
            var oldQuestionnireDocument = CreateQuestionnaireDocument();

            var histotyVersionsService = Mock.Of<IQuestionnireHistotyVersionsService>(s => s.GetByHistoryVersion(historyReferanceId) == oldQuestionnireDocument);
            var questionnaire = Create.Questionnaire(histotyVersionsService: histotyVersionsService);
            questionnaire.Initialize(questionnaireId, currentQuestionnireDocument, null);
            var command = Create.Command.RestoreVersionQuestionnaire(questionnaireId, historyReferanceId, personWhitoutPermissions);

            // Act
            var exception = Catch.Exception(() => questionnaire.RestoreVersion(command));

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.GetType().IsAssignableFrom(typeof(QuestionnaireException)), Is.True);
            Assert.That(((QuestionnaireException)exception).ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }
    }
}