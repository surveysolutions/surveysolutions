using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level1_and_linked_question_is_on_level1 : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId),
                    Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                    Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                })
            });
           
            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, "answer 0");
            BecauseOf();
        }

        private void BecauseOf() =>
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer 1");

        [NUnit.Framework.Test] public void should_linked_single_question_from_roster1_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(0));

            interview.GetLinkedSingleOptionQuestion(identity).Options.Count.Should().Be(2);
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0)).Should().Be("answer 0");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1)).Should().Be("answer 1");
        }

        [NUnit.Framework.Test] public void should_linked_single_question_from_roster2_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(1));
            interview.GetLinkedSingleOptionQuestion(identity).Options.Count.Should().Be(2);

            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0)).Should().Be("answer 0");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1)).Should().Be("answer 1");
        }

        [NUnit.Framework.Test] public void should_linked_multi_question_from_roster1_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedMultiQuestionId, Create.Entity.RosterVector(0));
            interview.GetLinkedMultiOptionQuestion(identity).Options.Count.Should().Be(2);

            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0)).Should().Be("answer 0");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1)).Should().Be("answer 1");
        }

        [NUnit.Framework.Test] public void should_linked_multi_question_from_roster2_has_2_options () 
        {
            var identity = Create.Entity.Identity(linkedMultiQuestionId, Create.Entity.RosterVector(1));

            interview.GetLinkedMultiOptionQuestion(identity).Options.Count.Should().Be(2);
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0)).Should().Be("answer 0");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(1)).Should().Be("answer 1");
            //answersToBeOptions.OfType<TextAnswer>().Select(x => x.Answer).ShouldContainOnly("answer 0", "answer 1");
        }

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");

        static readonly Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}
