using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    public class when_creating_interview_from_questionnaire_with_selected_default_translation: InterviewTestsContext
    {
        private Interview interview;
        private const string language = "Mova";
        private readonly Guid rosterId = Id.gA;

        private EventContext eventContext;

        [OneTimeSetUp]
        public void Setup()
        {
            var translationId = Id.gC;

            var nonTranslatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: Id.g8,
                chapterId: Id.g1,
                children: Create.Entity.FixedRoster(
                    rosterId: rosterId,
                    obsoleteFixedTitles: new List<string> { "title1", "title2" }
                )
            );

            nonTranslatedQuestionnaire.Translations.Add(Create.Entity.Translation(translationId, language));

            var translatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: Id.g8,
                chapterId: Id.g1,
                children: Create.Entity.FixedRoster(
                    rosterId: rosterId,
                    obsoleteFixedTitles: new List<string> { "тайтiль1", "тайтiль2" }
                ));

            translatedQuestionnaire.Translations.Add(Create.Entity.Translation(translationId, language));

            nonTranslatedQuestionnaire.DefaultTranslation = translationId;
            translatedQuestionnaire.DefaultTranslation = translationId;

            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), language) == translatedPlainQuestionnaire);

            // ACT
            interview = SetupStatefullInterviewWithExpressionStorageWithoutCreate(nonTranslatedQuestionnaire
                , questionnaireStorage: questionnaires);

            var command = Create.Command.CreateInterview(Guid.Empty, Id.g1, 
                new QuestionnaireIdentity(nonTranslatedQuestionnaire.PublicKey, 1),
                Id.g2, null, null, null);

            this.eventContext = new EventContext();
            interview.CreateInterview(command);
        }

        [Test]
        public void should_apply_switch_translation_event()
        {
            Assert.That(this.eventContext.GetSingleEvent<TranslationSwitched>().Language, Is.EqualTo(language));
        }
    }
}
