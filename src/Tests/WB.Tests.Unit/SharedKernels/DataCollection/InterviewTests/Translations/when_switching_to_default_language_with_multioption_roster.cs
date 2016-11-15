using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Translations
{
    [Ignore("KP-8159")]
    internal class when_switching_to_default_language_with_multioption_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestion = Guid.Parse("11111111111111111111111111111111");

            var nonTranslatedQuestionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: rosterSizeQuestion,
                    options: new List<Answer>
                    {
                        Create.Entity.Answer("title1", 1)
                    }),
                Create.Entity.Roster(
                    rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestion
                    ));

            nonTranslatedQuestionnaire.Translations.Add(
                    Create.Entity.Translation(translationId, targetLanguage));

            var translatedQuestionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Entity.MultyOptionsQuestion(id: rosterSizeQuestion,
                    options: new List<Answer>
                    {
                        Create.Entity.Answer("тайтл1", 1)
                    }),
                Create.Entity.Roster(
                    rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestion
                    ));

            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), targetLanguage) == translatedPlainQuestionnaire);

            interview = Create.AggregateRoot.Interview(
                questionnaireRepository: questionnaires
                );

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty, DateTime.Now, new [] {1});
            interview.SwitchTranslation(Create.Command.SwitchTranslation(language: targetLanguage));
            eventContext = new EventContext();
        };


        Because of = () => interview.SwitchTranslation(Create.Command.SwitchTranslation(language: null));

        It should_raise_roster_titles_changed_event_for_multioption_question = () =>
        {
            var @event = eventContext.GetSingleEvent<RosterInstancesTitleChanged>();
            @event.ChangedInstances.Length.ShouldEqual(1);
            @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId).ShouldBeTrue();
            @event.ChangedInstances[0].RosterInstance.RosterInstanceId.ShouldEqual(1);
            @event.ChangedInstances[0].Title.ShouldEqual("title1");
        };

        static Interview interview;
        static EventContext eventContext;
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string targetLanguage = "ru";
        static Guid rosterId;
        static Guid rosterSizeQuestion;
    }
}