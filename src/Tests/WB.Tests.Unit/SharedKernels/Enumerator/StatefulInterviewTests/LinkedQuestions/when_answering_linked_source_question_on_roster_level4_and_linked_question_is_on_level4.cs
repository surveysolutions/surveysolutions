using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level4_and_linked_question_is_on_level4 : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: rosterSizeQuestion1Id, variable: "q1"),
                Create.Entity.ListRoster(roster1Id, variable: "r1", rosterSizeQuestionId: rosterSizeQuestion1Id, children: new IComposite[]
                {
                    Create.Entity.TextListQuestion(questionId: rosterSizeQuestion2Id, variable: "q2"),
                    Create.Entity.ListRoster(roster2Id, variable: "r2", rosterSizeQuestionId: rosterSizeQuestion2Id, children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(questionId: rosterSizeQuestion3Id, variable: "q3"),
                        Create.Entity.ListRoster(roster3Id, variable: "r3", rosterSizeQuestionId: rosterSizeQuestion3Id, children: new IComposite[]
                        {
                            Create.Entity.TextListQuestion(questionId: rosterSizeQuestion4Id, variable: "q4"),
                            Create.Entity.ListRoster(roster4Id, variable: "r4", rosterSizeQuestionId: rosterSizeQuestion4Id, children: new IComposite[]
                            {
                                Create.Entity.SingleOptionQuestion(questionId: linkedSingleQuestionId, linkedToRosterId: roster4Id, variable: "q5"),
                                Create.Entity.MultipleOptionsQuestion(questionId: linkedMultiQuestionId, linkedToRosterId: roster4Id, variable: "q6"),
                            }),
                        }),
                    }),
                }),
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion1Id, RosterVector.Empty, DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1"),
                new Tuple<decimal, string>(2, "house 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion2Id, Create.Entity.RosterVector(1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1"),
                new Tuple<decimal, string>(2, "house 1 person 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion2Id, Create.Entity.RosterVector(2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1"),
                new Tuple<decimal, string>(2, "house 2 person 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Create.Entity.RosterVector(1, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1 pet 1"),
                new Tuple<decimal, string>(2, "house 1 person 1 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Create.Entity.RosterVector(1, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 2 pet 1"),
                new Tuple<decimal, string>(2, "house 1 person 2 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Create.Entity.RosterVector(2, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1 pet 1"),
                new Tuple<decimal, string>(2, "house 2 person 1 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Create.Entity.RosterVector(2, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 2 pet 1"),
                new Tuple<decimal, string>(2, "house 2 person 2 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(1, 1, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1 pet 1 worm 1"),
                new Tuple<decimal, string>(2, "house 1 person 1 pet 1 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(1, 1, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1 pet 2 worm 1"),
                new Tuple<decimal, string>(2, "house 1 person 1 pet 2 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(1, 2, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 2 pet 1 worm 1"),
                new Tuple<decimal, string>(2, "house 1 person 2 pet 1 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(1, 2, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 2 pet 2 worm 1"),
                new Tuple<decimal, string>(2, "house 1 person 2 pet 2 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(2, 1, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1 pet 1 worm 1"),
                new Tuple<decimal, string>(2, "house 2 person 1 pet 1 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(2, 1, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1 pet 1 worm 1"),
                new Tuple<decimal, string>(2, "house 2 person 1 pet 1 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(2, 1, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1 pet 2 worm 1"),
                new Tuple<decimal, string>(2, "house 2 person 1 pet 2 worm 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(2, 2, 1), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 2 pet 1 worm 1"),
                new Tuple<decimal, string>(2, "house 2 person 2 pet 1 worm 2"),
            });

            BecauseOf();
        }

        private void BecauseOf() =>
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion4Id, Create.Entity.RosterVector(2, 2, 2), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 2 pet 2 worm 1"),
                new Tuple<decimal, string>(2, "house 2 person 2 pet 2 worm 2"),
            });

        [NUnit.Framework.Test] public void should_linked_single_question_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(2, 2, 2, 2));
            var linkedSingleOptionQuestion = interview.GetLinkedSingleOptionQuestion(identity);
            linkedSingleOptionQuestion.Options.Count.Should().Be(2);
            
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.First()).Should().Be("house 2 person 2 pet 2 worm 1");
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.Second()).Should().Be("house 2 person 2 pet 2 worm 2");
        }

        [NUnit.Framework.Test] public void should_linked_multi_question_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(2, 2, 2, 2));
            var linkedSingleOptionQuestion = interview.GetLinkedSingleOptionQuestion(identity);
            linkedSingleOptionQuestion.Options.Count.Should().Be(2);
            
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.First()).Should().Be("house 2 person 2 pet 2 worm 1");
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.Second()).Should().Be("house 2 person 2 pet 2 worm 2");
        }

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid roster4Id = Guid.Parse("44444444444444444444444444444444");

        static readonly Guid rosterSizeQuestion1Id = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid rosterSizeQuestion2Id = Guid.Parse("66666666666666666666666666666666");
        static readonly Guid rosterSizeQuestion3Id = Guid.Parse("77777777777777777777777777777777");
        static readonly Guid rosterSizeQuestion4Id = Guid.Parse("88888888888888888888888888888888");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId  = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}
