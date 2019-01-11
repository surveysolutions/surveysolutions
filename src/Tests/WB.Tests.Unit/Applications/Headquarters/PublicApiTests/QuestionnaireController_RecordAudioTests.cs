﻿using System;
using System.Net;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class QuestionnaireController_RecordAudioTests : ApiTestContext
    {
        [Test]
        public void should_return_404_for_not_existing_questionnaire()
        {
            var controller = CreateQuestionnairesController(questionnaireBrowseItems: new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>());

            // Act
            var response = controller.RecordAudio(Guid.NewGuid(), 1, new RecordAudioRequest());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void should_return_404_for_deleted_questionnaire()
        {
            var questionnaires = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            questionnaires.Store(Create.Entity.QuestionnaireBrowseItem(Id.gA, 1, deleted: true), "questionnaire id");

            var controller = CreateQuestionnairesController(questionnaireBrowseItems: questionnaires);

            // Act
            var response = controller.RecordAudio(Id.gA, 1, new RecordAudioRequest());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void should_return_404_for_disabled_questionnaire()
        {
            var questionnaires = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            questionnaires.Store(Create.Entity.QuestionnaireBrowseItem(Id.gA, 1, disabled: true), "questionnaire id");

            var controller = CreateQuestionnairesController(questionnaireBrowseItems: questionnaires);

            // Act
            var response = controller.RecordAudio(Id.gA, 1, new RecordAudioRequest());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void should_set_questionnaire_enabled_recoring()
        {
            var questionnaires = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            var questionnaireBrowseItem = Create.Entity.QuestionnaireBrowseItem(Id.gA, 1);
            questionnaires.Store(questionnaireBrowseItem, Id.gA.FormatGuid());

            var controller = CreateQuestionnairesController(questionnaireBrowseItems: questionnaires);

            // Act
            var response = controller.RecordAudio(Id.gA, 1, new RecordAudioRequest
            {
                Enabled = true
            });

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(questionnaireBrowseItem.IsAudioRecordingEnabled, Is.True);
        }
    }
}
