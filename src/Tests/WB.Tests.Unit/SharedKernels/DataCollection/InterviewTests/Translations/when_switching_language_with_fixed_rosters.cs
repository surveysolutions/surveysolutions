using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Translations
{
    internal class when_switching_language_with_fixed_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var chapterId = Guid.Parse("11111111111111111111111111111111");

            var nonTranslatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.FixedRoster(
                rosterId: rosterId,
                obsoleteFixedTitles: new List<string> {"title1", "title2"}
                ));
            nonTranslatedQuestionnaire.Translations.Add(
                    Create.Entity.Translation(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), targetLanguage));

            var translatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.FixedRoster(
                rosterId: rosterId,
                obsoleteFixedTitles: new List<string> { "тайтл1", "тайтл2" }
                ));

            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), targetLanguage) == translatedPlainQuestionnaire);

            interview = Create.AggregateRoot.Interview(
                questionnaireRepository: questionnaires
                );
            var instance1 = Create.Entity.RosterVector(0);
            var instance2 = Create.Entity.RosterVector(1);

            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, instance1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, instance2));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(rosterId, instance1));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(rosterId, instance2));

            eventContext = new EventContext();
        };

        Because of = () => interview.SwitchTranslation(Create.Command.SwitchTranslation(language: targetLanguage));

        It should_raise_roster_titles_changed_event_for_fixed_roster = () =>
        {
            var @event = eventContext.GetSingleEvent<RosterInstancesTitleChanged>();
            @event.ChangedInstances.Length.ShouldEqual(2);
            @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId).ShouldBeTrue();
            @event.ChangedInstances[0].Title.ShouldEqual("тайтл1");
            @event.ChangedInstances[1].Title.ShouldEqual("тайтл2");
        };

        private static Interview interview;
        private static EventContext eventContext;
        private static string targetLanguage = "ru";
        private static Guid rosterId;
    }
}