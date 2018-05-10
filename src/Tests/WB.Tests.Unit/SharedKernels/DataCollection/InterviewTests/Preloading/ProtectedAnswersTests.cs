using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Preloading
{
    public class ProtectedAnswersTests : InterviewTestsContext
    {
        [Test]
        public void When_interview_created_with_protected_answers_Should_raise_event_about_it()
        {
            Guid multipleOptionsQuestionId = Id.g1;
            Guid textListQuestionId = Id.g2;
            Guid numericQuestionId = Id.g3;

            Guid userId = Id.gA;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(multipleOptionsQuestionId, answers: new int[] { 1, 2, 3 }, variable:"variable1"),
                Create.Entity.TextListQuestion(questionId: textListQuestionId, variable:"variable2"),
                Create.Entity.NumericIntegerQuestion(numericQuestionId, variable:"variable3"));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var multipleOptionsQuestionIdentity = Create.Identity(multipleOptionsQuestionId);
            var listQuestionIdentity = Create.Identity(textListQuestionId);
            var numericQuestionIdentity = Create.Identity(numericQuestionId);

            var preloadedMultipleOptionsAnswer = new[] {1};
            var preloadedTextListAnswer = Create.Entity.ListAnswer(5, 10);
            var preloadedNumericAnswer = 11;
            
            var command =
                Create.Command.CreateInterview(
                    questionnaire.PublicKey, 1,
                    null,
                    new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(multipleOptionsQuestionIdentity, Create.Entity.MultiOptionAnswer(preloadedMultipleOptionsAnswer)),
                        Create.Entity.InterviewAnswer(listQuestionIdentity, preloadedTextListAnswer),
                        Create.Entity.InterviewAnswer(numericQuestionIdentity, Create.Entity.NumericIntegerAnswer(preloadedNumericAnswer))
                    },
                    userId,
                    protectedAnswers: new List<string>{"variable1", "variable2", "variable3"});

            // Act
            using (EventContext eventContext = new EventContext())
            {
                interview.CreateInterview(command);

                var answersMarkedAsProtected = eventContext.GetEvent<AnswersMarkedAsProtected>();

                Assert.That(answersMarkedAsProtected.Questions, Does.Contain(multipleOptionsQuestionIdentity));
                Assert.That(answersMarkedAsProtected.Questions, Does.Contain(listQuestionIdentity));
                Assert.That(answersMarkedAsProtected.Questions, Does.Contain(numericQuestionIdentity));
            }

            var multipleOptionsTreeProtectedAnswers = interview.GetQuestion(multipleOptionsQuestionIdentity)
                .GetAsInterviewTreeMultiOptionQuestion().ProtectedAnswers;
            Assert.That(multipleOptionsTreeProtectedAnswers, Is.EqualTo(preloadedMultipleOptionsAnswer));

            var listProtectedQuestionProtectedAnswers = interview.GetQuestion(listQuestionIdentity).GetAsInterviewTreeTextListQuestion()
                .ProtectedAnswers;
            Assert.That(listProtectedQuestionProtectedAnswers.Select(x => x.Value), Is.EquivalentTo(preloadedTextListAnswer.Rows.Select(x => x.Value)));

            var numericProtectedAnswer = interview.GetQuestion(numericQuestionIdentity).GetAsInterviewTreeIntegerQuestion().ProtectedAnswer;

            Assert.That(numericProtectedAnswer, Is.EqualTo(preloadedNumericAnswer));
        }
    }
}
