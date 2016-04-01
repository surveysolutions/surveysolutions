using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_from_hq_interview_in_interviewer_assigned_status : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interviewSynchronizationDto = Create.InterviewSynchronizationDto(status: InterviewStatus.InterviewerAssigned);
            interview = Create.Interview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => interview.SynchronizeInterviewFromHeadquarters(interview.EventSourceId, interviewerId, supervisorId, interviewSynchronizationDto, DateTime.Now);

        It should_raise_InterviewCreated_event = () =>
           eventContext.ShouldContainEvent<InterviewCreated>();

        It should_raise_SupervisorAssigned_event = () =>
            eventContext.ShouldContainEvent<SupervisorAssigned>(evnt=>evnt.SupervisorId==supervisorId && evnt.UserId==supervisorId);

        It should_raise_InterviewerAssigned_event = () =>
          eventContext.ShouldContainEvent<InterviewerAssigned>(evnt => evnt.InterviewerId == interviewerId && evnt.UserId == supervisorId);

        private static Interview interview;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static EventContext eventContext;
        private static Guid supervisorId=Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
    }
}