using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    public class MultipleOptionsQuestionWithProtectedAnswersTests : InterviewTestsContext
    {
        private StatefulInterview interview;
        readonly Guid questionId = Id.g1;
        readonly Guid userId = Id.gA;

        [SetUp]
        public void Setup()
        {
            var variableName = "protected";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, answers: new[] { 1, 2, 3 }, variable: variableName));

            interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var questionIdentity = Create.Identity(questionId);
            var command = Create.Command.CreateInterview(
                questionnaire.PublicKey, 1,
                null,
                new List<InterviewAnswer>
                {
                    Create.Entity.InterviewAnswer(questionIdentity, Create.Entity.MultiOptionAnswer(1))
                },
                userId,
                protectedAnswers: new List<string> { variableName });

            interview.CreateInterview(command);
        }

        [Test]
        public void When_answer_extends_existing_protected_answer_Should_allow()
        {
            using (EventContext eventContext = new EventContext())
            {
                interview.AnswerMultipleOptionsQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, new[] { 1, 2 });

                eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>();
            }

            var question = interview.GetQuestion(Create.Identity(questionId)).GetAsInterviewTreeMultiOptionQuestion();

            Assert.That(question.ProtectedAnswers, Is.EquivalentTo(1.ToEnumerable()));
        }

        [Test]
        public void should_throw_when_protected_answer_is_removed()
        {
            TestDelegate act = () => interview.AnswerMultipleOptionsQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, new[] { 3 });

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing protected answer is not allowed"));
        }

        [Test]
        public void should_not_allow_removing_protected_answer()
        {
            TestDelegate act = () => interview.RemoveAnswer(questionId, RosterVector.Empty, userId, DateTime.UtcNow);

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing protected answer is not allowed"));
        }
    }
}
