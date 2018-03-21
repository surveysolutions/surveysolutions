using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_fixed_roster_has_substitution : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId: questionId, variable: "subst"), 
                Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                    rosterId: rosterId,
                    fixedRosterTitles: new[]
                    {
                        Create.Entity.FixedTitle(1, "one"),
                        Create.Entity.FixedTitle(2, "two")
                    },
                    title: "Roster %subst%"));

            Guid questionnaireId = Guid.NewGuid();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            events = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() => 
            interview.AnswerTextQuestion(Guid.NewGuid(), questionId, Empty.RosterVector, DateTime.Now, "foo");

        [NUnit.Framework.Test] public void should_raise_title_changed_event_for_group_after_answer () 
        {
            var substitutionTitlesChanged = events.GetEvent<SubstitutionTitlesChanged>();
            substitutionTitlesChanged.Should().NotBeNull();
            substitutionTitlesChanged.Groups.Length.Should().Be(2);

            substitutionTitlesChanged.Groups[0].Should().Be(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)));
            substitutionTitlesChanged.Groups[1].Should().Be(Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2)));
        }

        static Guid questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Interview interview;
        static EventContext events;
    }
}
