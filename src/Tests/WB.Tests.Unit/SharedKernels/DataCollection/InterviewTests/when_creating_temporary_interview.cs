using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    public class when_creating_temporary_interview : InterviewTestsContext
    {
        [Test]
        public void should_create_interview_that_can_accept_answers()
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                var interviewId = Id.g1;
                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionnaireVersion = 18;
                
                var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                    => _.Version == questionnaireVersion);

                var command = Create.Command.CreateTemporaryInterview(interviewId, userId, Create.Entity.QuestionnaireIdentity(questionnaireId, questionnaireVersion));

                var interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

                //Act
                interview.CreateTemporaryInterview(command);

                // Assert
                eventContext.ShouldContainEvent<InterviewCreated>(e => e.AssignmentId == null && e.QuestionnaireId == questionnaireId && e.QuestionnaireVersion == questionnaireVersion);
                eventContext.ShouldContainEvent<SupervisorAssigned>(e => e.SupervisorId == userId);
                eventContext.ShouldContainEvent<InterviewStatusChanged>(e => e.Status == InterviewStatus.InterviewerAssigned);
            }
        }
    }
}
