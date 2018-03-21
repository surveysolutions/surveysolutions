using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Translations
{
    internal class when_switching_to_default_language_with_multioption_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestion = Guid.Parse("11111111111111111111111111111111");
            var chapterId = Guid.Parse("22222222222222222222222222222222");

            var nonTranslatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId,
                Create.Entity.MultyOptionsQuestion(id: rosterSizeQuestion, options: new List<Answer> { Create.Entity.Answer("title1", 1) }),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestion));

            nonTranslatedQuestionnaire.Translations.Add(Create.Entity.Translation(translationId, targetLanguage));

            var translatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId,
                Create.Entity.MultyOptionsQuestion(id: rosterSizeQuestion, options: new List<Answer> { Create.Entity.Answer("тайтл1", 1) }),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestion));

            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), targetLanguage) == translatedPlainQuestionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaires);

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty, DateTime.Now, new [] {1});
            interview.SwitchTranslation(Create.Command.SwitchTranslation(language: targetLanguage));
            eventContext = new EventContext();
            BecauseOf();
        }


        public void BecauseOf() => interview.SwitchTranslation(Create.Command.SwitchTranslation(language: null));

        [NUnit.Framework.Test] public void should_raise_roster_titles_changed_event_for_multioption_question () 
        {
            var @event = eventContext.GetSingleEvent<RosterInstancesTitleChanged>();
            @event.ChangedInstances.Length.Should().Be(1);
            @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId).Should().BeTrue();
            @event.ChangedInstances[0].RosterInstance.RosterInstanceId.Should().Be(1);
            @event.ChangedInstances[0].Title.Should().Be("title1");
        }

        static Interview interview;
        static EventContext eventContext;
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string targetLanguage = "ru";
        static Guid rosterId;
        static Guid rosterSizeQuestion;
    }
}
