using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    [TestFixture]
    [TestOf(typeof(StatefulInterview))]
    internal class when_getting_answer_as_string_for_question_linked_to_list_question
    {
        private StatefulInterview interview;
        readonly Guid linkedSingleOptionQuestionid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        readonly Guid linkedMultiOptionQuestionid  = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        readonly Guid textWithSubstitutionOnSingleQuestionid = Guid.Parse("11111111111111111111111111111111");
        readonly Guid textWithSubstitutionOnMultiQuestionid = Guid.Parse("22222222222222222222222222222222");
        private readonly Guid userId = Guid.NewGuid();

        [SetUp]
        public void Context()
        {
            Guid textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterId = Guid.Parse("11111111111111111111111111111111");
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(textListQuestionId, variable: "q1"),
                    Create.Entity.SingleOptionQuestion(this.linkedSingleOptionQuestionid, variable: "single", linkedToQuestionId: textListQuestionId, answers: new List<Answer>()),
                    Create.Entity.MultyOptionsQuestion(this.linkedMultiOptionQuestionid, variable: "multi", linkedToQuestionId: textListQuestionId, options: new List<Answer>()),
                    Create.Entity.TextQuestion(textWithSubstitutionOnSingleQuestionid, variable: "q2", text: "Result: %single%"),
                    Create.Entity.TextQuestion(textWithSubstitutionOnMultiQuestionid, variable: "q3", text: "Result: %multi%"));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                Tuple.Create(0m, "zero"),
                Tuple.Create(1m, "one"),
                Tuple.Create(2m, "two"),
            });
        }

        [Test]
        public void Should_return_null_as_answer_to_unanswered_single_option_question()
        {
            var singleOpionAnswer = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedSingleOptionQuestionid));
            Assert.That(singleOpionAnswer, Is.Null);
        }

        [Test]
        public void Should_return_null_as_answer_to_unanswered_multi_option_question()
        {
            var multioptionAnswer = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedMultiOptionQuestionid));
            Assert.That(multioptionAnswer, Is.Null);
        }

        [Test]
        public void Should_return_answer_string_value_when_linked_single_option_question_answered()
        {
            interview.AnswerSingleOptionQuestion(userId, this.linkedSingleOptionQuestionid, RosterVector.Empty, DateTime.Now, 1);

            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedSingleOptionQuestionid));
            Assert.That(answerAsString, Is.EqualTo("one"));

            var question = this.interview.GetQuestion(Create.Entity.Identity(this.textWithSubstitutionOnSingleQuestionid));
            Assert.That(question.Title.Text, Is.EqualTo("Result: one"));
        }

        [Test]
        public void Should_return_answer_string_value_when_linked_multi_option_question_answered()
        {
            interview.AnswerMultipleOptionsQuestion(userId, this.linkedMultiOptionQuestionid, RosterVector.Empty, DateTime.Now, new int[] {1, 2});

            var answerAsString = this.interview.GetAnswerAsString(Create.Entity.Identity(this.linkedMultiOptionQuestionid));
            Assert.That(answerAsString, Is.EqualTo("one, two"));

            var question = this.interview.GetQuestion(Create.Entity.Identity(this.textWithSubstitutionOnMultiQuestionid));
            Assert.That(question.Title.Text, Is.EqualTo("Result: one, two"));
        }
    }
}