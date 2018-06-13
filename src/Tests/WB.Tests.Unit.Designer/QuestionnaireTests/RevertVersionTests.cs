﻿using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;


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
    }
}
