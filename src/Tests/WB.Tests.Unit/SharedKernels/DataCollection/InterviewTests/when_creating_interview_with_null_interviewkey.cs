using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_null_interviewkey : InterviewTestsContext
    {
        [Test]
        public void should_not_set_empty_interview_key()
        {
            Guid interviewId = Id.g1;
            
            var command = Create.Command.CreateInterview(interviewId, Id.g2, Id.g3, 1, new List<InterviewAnswer>(), Id.g4, Id.g5, null, null);

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false, interviewId: interviewId, questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());
            using (EventContext eventContext = new EventContext())
            {
                interview.CreateInterview(command);

                eventContext.ShouldNotContainEvent<InterviewKeyAssigned>();
            }
        }
    }
}
