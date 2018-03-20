using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level3_and_linked_question_is_on_level3 : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestion1Id),
                Abc.Create.Entity.Roster(roster1Id, variable:"roster1Id", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestion1Id, children: new IComposite[]
                {
                    Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestion2Id),
                    Abc.Create.Entity.Roster(roster2Id, variable:"roster2Id", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestion2Id, children: new IComposite[]
                    {
                        Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestion3Id),
                        Abc.Create.Entity.Roster(roster3Id, variable:"roster3Id", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestion3Id, children: new IComposite[]
                        {
                            Abc.Create.Entity.SingleOptionQuestion(questionId: linkedSingleQuestionId, linkedToRosterId: roster3Id, variable: null),
                            Abc.Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToRosterId: roster3Id, variable: "multi"),
                        }),
                    }),
                }),
            });

            interview = SetupStatefullInterview(questionnaireDocument);

            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion1Id, RosterVector.Empty, DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1"),
                new Tuple<decimal, string>(2, "house 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion2Id, Abc.Create.Entity.RosterVector(new[] {1}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1"),
                new Tuple<decimal, string>(2, "house 1 person 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion2Id, Abc.Create.Entity.RosterVector(new[] {2}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1"),
                new Tuple<decimal, string>(2, "house 2 person 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Abc.Create.Entity.RosterVector(new[] {1, 1}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 1 pet 1"),
                new Tuple<decimal, string>(2, "house 1 person 1 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Abc.Create.Entity.RosterVector(new[] {1, 2}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 1 person 2 pet 1"),
                new Tuple<decimal, string>(2, "house 1 person 2 pet 2"),
            });
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Abc.Create.Entity.RosterVector(new[] {2, 1}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 1 pet 1"),
                new Tuple<decimal, string>(2, "house 2 person 1 pet 2"),
            });

            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerTextListQuestion(interviewerId, rosterSizeQuestion3Id, Abc.Create.Entity.RosterVector(new[] {2, 2}), DateTime.UtcNow, new[]
            {
                new Tuple<decimal, string>(1, "house 2 person 2 pet 1"),
                new Tuple<decimal, string>(2, "house 2 person 2 pet 2"),
            });

        [NUnit.Framework.Test] public void should_linked_single_question_has_2_options () 
        {
            var identity = Abc.Create.Identity(linkedSingleQuestionId, Abc.Create.Entity.RosterVector(new[] {2, 2, 2}));
            var linkedSingleOptionQuestion = interview.GetLinkedSingleOptionQuestion(identity);
            linkedSingleOptionQuestion.Options.Count.Should().Be(2);
            
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.First()).Should().Be("house 2 person 2 pet 1");
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.Last()).Should().Be("house 2 person 2 pet 2");
        }

        [NUnit.Framework.Test] public void should_linked_multi_question_has_2_options () 
        {
            var identity = Abc.Create.Identity(linkedSingleQuestionId, Abc.Create.Entity.RosterVector(new[] {2, 2, 2}));
            var linkedSingleOptionQuestion = interview.GetLinkedSingleOptionQuestion(identity);
            linkedSingleOptionQuestion.Options.Count.Should().Be(2);
            
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.First()).Should().Be("house 2 person 2 pet 1");
            interview.GetLinkedOptionTitle(identity, linkedSingleOptionQuestion.Options.Last()).Should().Be("house 2 person 2 pet 2");
        }

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid rosterSizeQuestion1Id = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid rosterSizeQuestion2Id = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid rosterSizeQuestion3Id = Guid.Parse("77777777777777777777777777777777");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId  = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}
