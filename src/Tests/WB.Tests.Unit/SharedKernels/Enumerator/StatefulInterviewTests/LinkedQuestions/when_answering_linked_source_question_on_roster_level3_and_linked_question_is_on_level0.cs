using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level3_and_linked_question_is_on_level0 : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "q1"),
                Create.Entity.Roster(rosterId: roster1Id, variable: "r1", rosterSizeQuestionId: rosterSizeQuestionId, rosterTitleQuestionId: numericTitleQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: numericTitleQuestionId, variable: "q2"),
                    Create.Entity.MultipleOptionsQuestion(questionId: nestedRosterSizeQuestionId, variable: "q3", textAnswers: new []{ Create.Entity.Option("1", "Multi 1"), Create.Entity.Option("2", "Multi 2") }  ),
                    Create.Entity.Roster(rosterId: roster2Id, variable: "r2", rosterSizeQuestionId: nestedRosterSizeQuestionId, children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(roster3Id, variable: "r3", fixedTitles: new [] { Create.Entity.FixedTitle(100, "Title 1"), Create.Entity.FixedTitle(200, "Title 2") }, children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId, variable: "q4")
                        })
                    })
                }),
                Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q5"),
                Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q6")
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);
            interview.AnswerTextQuestion(interviewerId, numericTitleQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, "numeric 1");
            interview.AnswerTextQuestion(interviewerId, numericTitleQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "numeric 2");
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, new[] { 1, 2 });
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, new[] { 1, 2 });

            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 1, 100), DateTime.UtcNow, "answer 0.1.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 1, 200), DateTime.UtcNow, "answer 0.1.200");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 2, 100), DateTime.UtcNow, "answer 0.2.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 2, 200), DateTime.UtcNow, "answer 0.2.200");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 1, 100), DateTime.UtcNow, "answer 1.3.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 1, 200), DateTime.UtcNow, "answer 1.3.200");
            BecauseOf();
        }

        private void BecauseOf() =>
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 2, 100), DateTime.UtcNow, "answer 1.4.100");

        [NUnit.Framework.Test] public void should_linked_single_question_has_7_options () 
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, RosterVector.Empty);

            interview.GetLinkedSingleOptionQuestion(identity).Options.Count.Should().Be(7);
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1, 100)).Should().Be("numeric 1: Multi 1: Title 1: answer 0.1.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1, 200)).Should().Be("numeric 1: Multi 1: Title 2: answer 0.1.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2, 100)).Should().Be("numeric 1: Multi 2: Title 1: answer 0.2.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2, 200)).Should().Be("numeric 1: Multi 2: Title 2: answer 0.2.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 1, 100)).Should().Be("numeric 2: Multi 1: Title 1: answer 1.3.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 1, 200)).Should().Be("numeric 2: Multi 1: Title 2: answer 1.3.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 2, 100)).Should().Be("numeric 2: Multi 2: Title 1: answer 1.4.100");

        }

        [NUnit.Framework.Test] public void should_linked_multi_question_has_7_options () {
            var identity = Create.Entity.Identity(linkedMultiQuestionId, RosterVector.Empty);

            interview.GetLinkedMultiOptionQuestion(identity).Options.Count.Should().Be(7);
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1, 100)).Should().Be("numeric 1: Multi 1: Title 1: answer 0.1.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1, 200)).Should().Be("numeric 1: Multi 1: Title 2: answer 0.1.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2, 100)).Should().Be("numeric 1: Multi 2: Title 1: answer 0.2.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2, 200)).Should().Be("numeric 1: Multi 2: Title 2: answer 0.2.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 1, 100)).Should().Be("numeric 2: Multi 1: Title 1: answer 1.3.100");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 1, 200)).Should().Be("numeric 2: Multi 1: Title 2: answer 1.3.200");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1, 2, 100)).Should().Be("numeric 2: Multi 2: Title 1: answer 1.4.100");
        }

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid nestedRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid numericTitleQuestionId = Guid.Parse("77777777777777777777777777777777");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}
