using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    [TestFixture]
    [TestOf(typeof(StatefulInterview))]
    internal class when_getting_answer_as_string_for_question_linked_to_roster
    {
        private StatefulInterview interview;
        readonly Guid linkedSingleOptionQuestionid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        readonly Guid linkedMultiOptionQuestionid = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private readonly Guid userId = Guid.NewGuid();

        [SetUp]
        public void Context()
        {
            Guid textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterId = Guid.Parse("11111111111111111111111111111111");
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(textListQuestionId),
                    Create.Entity.Roster(rosterId, rosterSizeQuestionId: textListQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question),
                    Create.Entity.SingleOptionQuestion(this.linkedSingleOptionQuestionid, linkedToRosterId: rosterId),
                    Create.Entity.MultyOptionsQuestion(this.linkedMultiOptionQuestionid, linkedToRosterId: rosterId));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));

            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(0m, "zero")});
        }

        [Test]
        public void Should_return_null_as_answer_to_unanswered_single_option_question()
        {
            var singleOpionAnswer = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedSingleOptionQuestionid));
            Assert.That(singleOpionAnswer, Is.Empty);
        }

        [Test]
        public void Should_return_null_as_answer_to_unanswered_multi_option_question()
        {
            var multioptionAnswer = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedMultiOptionQuestionid));
            Assert.That(multioptionAnswer, Is.Empty);
        }

        [Test]
        public void Should_return_answer_string_value_when_linked_single_option_question_answered()
        {
            interview.AnswerSingleOptionLinkedQuestion(userId, this.linkedSingleOptionQuestionid, RosterVector.Empty, DateTime.Now, new[] { 0m });

            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedSingleOptionQuestionid));
            Assert.That(answerAsString, Is.EqualTo("zero"));
        }

        [Test]
        public void Should_return_answer_string_value_when_linked_multi_option_question_answered()
        {
            interview.AnswerMultipleOptionsLinkedQuestion(userId, this.linkedMultiOptionQuestionid, RosterVector.Empty, DateTime.Now, new[] { new decimal[] {0}});

            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedMultiOptionQuestionid));
            Assert.That(answerAsString, Is.EqualTo("zero"));
        }
    }
}