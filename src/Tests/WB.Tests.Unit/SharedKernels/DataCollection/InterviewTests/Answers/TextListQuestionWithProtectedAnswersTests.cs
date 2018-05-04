using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TextListQuestionWithProtectedAnswersTests : InterviewTestsContext
    {
        Guid questionId = Id.g1;
        Guid userId = Id.gA;
        private StatefulInterview interview;
        private TextListAnswer preloadedAnswer;

        [SetUp]
        public void Should_not_allow_removing_protected_answer()
        {
            Guid questionId = Id.g1;
            Guid userId = Id.gA;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId));

            interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var questionIdentity = Create.Identity(questionId);
            preloadedAnswer = Create.Entity.ListAnswer(1, 2);
            var command = Create.Command.CreateInterview(
                questionnaire.PublicKey, 1,
                null,
                new List<InterviewAnswer>
                {
                    Create.Entity.InterviewAnswer(questionIdentity, preloadedAnswer)
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
                Tuple<decimal, string>[] newAnswer = preloadedAnswer.ToTupleArray().Append(Tuple.Create(125m, "Test answer")).ToArray();
                
                interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, newAnswer);

                eventContext.ShouldContainEvent<TextListQuestionAnswered>();
            }
        }

        [Test]
        public void should_throw_when_protected_answer_is_reduced()
        {
            TestDelegate act = () => interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, preloadedAnswer.ToTupleArray().Take(1).ToArray());

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing or modification of protected answer is not allowed"));
        }

        [Test]
        public void should_not_allow_removing_protected_answer()
        {
            TestDelegate act = () => interview.RemoveAnswer(questionId, RosterVector.Empty, userId, DateTime.UtcNow);

            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing protected answer is not allowed"));
        }
    }
}
