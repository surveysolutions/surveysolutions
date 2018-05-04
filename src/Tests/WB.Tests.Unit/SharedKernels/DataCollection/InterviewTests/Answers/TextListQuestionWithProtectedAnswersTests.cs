using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    public class TextListQuestionWithProtectedAnswersTests : InterviewTestsContext
    {

        [Test]
        public void Should_not_allow_removing_protected_answer()
        {
            Guid questionId = Id.g1;
            Guid userId = Id.gA;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var questionIdentity = Create.Identity(questionId);
            var preloadedAnswer = Create.Entity.ListAnswer(1, 2);
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

            // Act
            TestDelegate act = () => interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, preloadedAnswer.ToTupleArray().Take(1).ToArray());

            // Assert
            Assert.That(act, Throws.Exception.TypeOf<InterviewException>().With.Message.EqualTo("Removing or modification of protected answer is not allowed"));
        }
    }
}
