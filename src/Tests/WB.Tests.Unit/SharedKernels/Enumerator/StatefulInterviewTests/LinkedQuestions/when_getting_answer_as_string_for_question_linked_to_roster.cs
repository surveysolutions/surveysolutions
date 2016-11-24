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
        readonly Guid linkedQuestionid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        [SetUp]
        public void Context()
        {
            Guid textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(textListQuestionId),
                    Create.Entity.Roster(rosterId, rosterSizeQuestionId: textListQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question),
                    Create.Entity.SingleOptionQuestion(linkedQuestionid, linkedToRosterId: rosterId));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));

            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(0m, "zero")});
        }

        [Test]
        public void Should_return_null_as_answer_to_single_option_question()
        {
            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedQuestionid));
            Assert.That(answerAsString, Is.Null);
        }

        [Test]
        public void Should_return_answer_string_value_when_linked_question_answered()
        {
            interview.AnswerSingleOptionLinkedQuestion(Guid.NewGuid(), this.linkedQuestionid, RosterVector.Empty, DateTime.Now, new[] { 0m });

            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedQuestionid));
            Assert.That(answerAsString, Is.EqualTo("zero"));
        }
    }
}