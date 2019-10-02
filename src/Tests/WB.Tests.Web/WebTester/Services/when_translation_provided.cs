using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;

using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_translation_provided : AppdomainsPerInterviewManagerTestsBase
    {
        private AppdomainsPerInterviewManager manager;
        private Guid interviewId = Guid.NewGuid();
        private Guid multiOptionQuestion = Id.g1;
        private Guid roster = Id.g2;
        
        [SetUp]
        public void Setup()
        {
            manager = CreateManager();
            interviewId = Id.gA;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapterAndLanguages(Id.gA,
                new [] { "Русский" }, new IComposite[]
                {
                    Create.Entity.MultyOptionsQuestion(multiOptionQuestion, new[]
                    {
                        Create.Entity.Option("1", "Rick"),
                        Create.Entity.Option("2", "Morty")
                    }),
                    Create.Entity.Roster(roster, "Title", 
                        rosterTitleQuestionId: multiOptionQuestion,
                        rosterSizeQuestionId: multiOptionQuestion,
                        rosterSizeSourceType: RosterSizeSourceType.Question)
                }
            );

            var translationId = questionnaire.Translations.Single().Id;

            this.SetupAppDomainInterview(manager, interviewId, questionnaire, new List<TranslationDto>
            {
                Create.Entity.TranslationInstance("Рик",   translationId, entityId: multiOptionQuestion, translationIndex: "1", type: TranslationType.OptionTitle),
                Create.Entity.TranslationInstance("Морти", translationId, entityId: multiOptionQuestion, translationIndex: "2", type: TranslationType.OptionTitle)
            });
        }

        [TearDown]
        public void TearDown() => manager.TearDown(interviewId);

        [Test]
        public void should_generate_events_with_proper_translation()
        {
            manager.Execute(Create.Command.SwitchTranslation("Русский", interviewId));
            var events = manager.Execute(Create.Command.AnswerMultipleOptionsQuestionCommand(interviewId, Guid.NewGuid(), new [] {1, 2}, multiOptionQuestion));

            var rosterTitleChanged = events.Select(ev => ev.Payload).OfType<RosterInstancesTitleChanged>().Single();

            Assert.That(rosterTitleChanged.ChangedInstances[0].Title, Is.EqualTo("Рик"));
            Assert.That(rosterTitleChanged.ChangedInstances[1].Title, Is.EqualTo("Морти"));
        }
    }
}
