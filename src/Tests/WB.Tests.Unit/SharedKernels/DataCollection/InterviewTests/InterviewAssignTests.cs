using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(Interview))]
    [TestFixture]
    internal class InterviewAssignTests : InterviewTestsContext
    {
        EventContext eventContext;

        public Interview SetupInterview()
        {
            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            return CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        }

        public void SetupEventContext()
        {
            eventContext = Create.Other.EventContext();
        }

        [TearDown]
        public void CleanTests()
        {
            eventContext?.Dispose();
            eventContext = null;
        }

        [Test]
        public void When_Interview_in_status_SupervisorAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));
            SetupEventContext();

            // act
            interview.AssignSupervisor(supervisorId, supervisorId2);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
            eventContext.AssertThatDoesNotContainEvent<InterviewerAssigned>();
        }


        [Test]
        public void Interview_in_status_InterviewerAssigned_And_interview_being_reassigned_within_one_team_As_result_interviewer_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            SetupEventContext();

            // act
            interview.AssignInterviewer(supervisorId, interviewerId2, DateTime.UtcNow);

            // assert
            eventContext.AssertThatContainsEvent<InterviewerAssigned>();
            eventContext.AssertThatDoesNotContainEvent<SupervisorAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
        }

        [Test]
        public void Interview_in_status_RejectedBySupervisor_And_interview_being_reassigned_within_one_team_As_result_interviewer_should_be_changed_Status_should_be_intact()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewApproved(supervisorId));
            interview.Apply(Create.Event.InterviewRejectedByHQ(headquarterId));
            interview.Apply(Create.Event.InterviewRejected(supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.RejectedBySupervisor));
            SetupEventContext();

            // act
            interview.AssignInterviewer(supervisorId, interviewerId, DateTime.UtcNow);

            // assert
            eventContext.AssertThatContainsEvent<InterviewerAssigned>();
            eventContext.AssertThatDoesNotContainEvent<SupervisorAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
        }


        [Test]
        public void Interview_in_status_Completed_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewApproved(supervisorId));
            interview.Apply(Create.Event.InterviewRejectedByHQ(headquarterId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters));
            SetupEventContext();

            // act
            interview.AssignSupervisor(headquarterId, supervisorId2);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewerAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
        }


        [Test]
        public void Interview_in_status_RejectedByHQ_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.Completed));
            SetupEventContext();

            // act
            interview.AssignSupervisor(headquarterId, supervisorId2);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewerAssigned>();
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
        }


        readonly Guid interviewerId   = Guid.Parse("99999999999999999999999999999999");
        readonly Guid interviewerId2  = Guid.Parse("22222222222222222222222222222222");
        readonly Guid supervisorId    = Guid.Parse("11111111111111111111111111111111");
        readonly Guid supervisorId2   = Guid.Parse("88888888888888888888888888888888");
        readonly Guid headquarterId   = Guid.Parse("77777777777777777777777777777777");
        readonly Guid questionnaireId = Guid.Parse("33333333333333333333333333333333");
    }

    internal static class EventContextExtension
    {
        [DebuggerStepThrough]
        public static void AssertThatContainsEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            bool isExists = condition == null
                ? eventContext.Events.Any(@event => @event.Payload is TEvent)
                : eventContext.Events.Any(@event => @event.Payload is TEvent && condition.Invoke((TEvent)@event.Payload));
            Assert.That<bool>(isExists, Is.True);
        }

        [DebuggerStepThrough]
        public static void AssertThatDoesNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            bool isExists = condition == null
                ? eventContext.Events.Any(@event => @event.Payload is TEvent)
                : eventContext.Events.Any(@event => @event.Payload is TEvent && condition.Invoke((TEvent)@event.Payload));
            Assert.That<bool>(isExists, Is.False);
        }
    }
}