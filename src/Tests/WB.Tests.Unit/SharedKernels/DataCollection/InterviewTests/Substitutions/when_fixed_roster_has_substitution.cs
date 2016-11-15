using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_fixed_roster_has_substitution : InterviewTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId: questionId, variable: "subst"), 
                Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                    rosterId: rosterId,
                    fixedRosterTitles: new[]
                    {
                        Create.Entity.FixedRosterTitle(1, "one"),
                        Create.Entity.FixedRosterTitle(2, "two")
                    },
                    title: "Roster %subst%"));

            Guid questionnaireId = Guid.NewGuid();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            events = new EventContext();
        };

        Because of = () => 
            interview.AnswerTextQuestion(Guid.NewGuid(), questionId, Empty.RosterVector, DateTime.Now, "foo");

        It should_raise_title_changed_event_for_group_after_answer = () =>
        {
            var substitutionTitlesChanged = events.GetEvent<SubstitutionTitlesChanged>();
            substitutionTitlesChanged.ShouldNotBeNull();
            substitutionTitlesChanged.Groups.Length.ShouldEqual(2);

            substitutionTitlesChanged.Groups[0].ShouldEqual(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)));
            substitutionTitlesChanged.Groups[1].ShouldEqual(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2)));
        };

        static Guid questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Interview interview;
        static EventContext events;
    }
}