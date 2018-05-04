using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    public class NumericIntegerQuestionWithProtectedAnswersTests : InterviewTestsContext
    {
        private StatefulInterview interview;
        readonly Guid questionId = Id.g1;
        readonly Guid userId = Id.gA;
        private readonly int preloadedAnswer = 5;

        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId));

            interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var questionIdentity = Create.Identity(questionId);
            var command = Create.Command.CreateInterview(
                questionnaire.PublicKey, 1,
                null,
                new List<InterviewAnswer>
                {
                    Create.Entity.InterviewAnswer(questionIdentity, Create.Entity.NumericIntegerAnswer(preloadedAnswer))
                },
                userId,
                protectedAnswers: new List<Identity> { questionIdentity });

            interview.CreateInterview(command);
        }

        [Test]
        public void When_answer_extends_existing_protected_answer_Should_allow()
        {
            using (EventContext eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, preloadedAnswer + 1);

                eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>();
            }
        }

        [Test]
        public void should_throw_when_protected_answer_is_reduced()
        {
            TestDelegate act = () => interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, preloadedAnswer - 1); ;

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Reduce value of protected answer is not allowed"));
        }

        [Test]
        public void should_not_allow_removing_protected_answer()
        {
            TestDelegate act = () => interview.RemoveAnswer(questionId, RosterVector.Empty, userId, DateTime.UtcNow);

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing protected answer is not allowed"));
        }
    }
}
