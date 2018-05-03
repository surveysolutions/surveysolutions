using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using NHibernate.Util;
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
            Guid questionId = Id.g1;
            Guid userId = Id.gA;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, answers: new int[] { 1, 2, 3 }));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
                questionnaire: questionnaire);

            var questionIdentity = Create.Identity(questionId);
            var command =
                Create.Command.CreateInterview(
                    questionnaire.PublicKey, 1,
                    null,
                    new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(questionIdentity, Create.Entity.MultiOptionAnswer(1))
                    },
                    userId,
                    protectedAnswers: new List<Identity>{questionIdentity});

            // Act
            using (EventContext eventContext = new EventContext())
            {
                interview.CreateInterview(command);

                var fixedMultipleOptionsEvent = eventContext.GetEvent<AnswersMarkedAsProtected>();

                Assert.That(fixedMultipleOptionsEvent.Questions, Does.Contain(questionIdentity));
            }
        }
    }
}
