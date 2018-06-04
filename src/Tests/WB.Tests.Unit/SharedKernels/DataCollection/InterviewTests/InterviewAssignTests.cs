using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(Interview))]
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
            eventContext = new EventContext();
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
            interview.AssignSupervisor(supervisorId, supervisorId2, DateTimeOffset.Now);

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
            interview.AssignSupervisor(headquarterId, supervisorId2, DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewerAssigned>(e => e.InterviewerId == null);
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
            interview.AssignSupervisor(headquarterId, supervisorId2, DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewerAssigned>(e => e.InterviewerId == null);
            eventContext.AssertThatDoesNotContainEvent<InterviewStatusChanged>();
        }

        [Test]
        public void Interview_in_status_InterviewerAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null_and_status_set_to_SupervisorAssigned()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            SetupEventContext();

            // act
            interview.AssignSupervisor(headquarterId, supervisorId2, DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>(s => s.Status == InterviewStatus.SupervisorAssigned);
            eventContext.AssertThatContainsEvent<InterviewerAssigned>(e => e.InterviewerId == null);
        }

        [Test]
        public void Interview_that_were_restarted_in_status_InterviewerAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_new_one_and_status_set_to_InterviewerAssigned()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(interview.EventSourceId, headquarterId, interviewerId2));

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>(s => s.Status == InterviewStatus.SupervisorAssigned);
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>(s => s.Status == InterviewStatus.InterviewerAssigned);
            eventContext.AssertThatContainsEvent<InterviewerAssigned>(e => e.InterviewerId == interviewerId2);
        }

        [Test]
        public void Interview_that_were_rejected_InterviewerAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null_and_status_set_to_SupervisorAssigned()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewRejected(supervisorId, null, DateTime.UtcNow));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(interview.EventSourceId, headquarterId, interviewerId: interviewerId2, supervisorId: supervisorId2));

            // assert
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>(s => s.Status == InterviewStatus.SupervisorAssigned);
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>(s => s.Status == InterviewStatus.InterviewerAssigned);
        }

        [Test]
        public void Interview_in_status_InterviewerAssigned_And_interview_being_reassigned_to_different_responsible_in_one_team_As_result_interviewer_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId, interviewerId: interviewerId2));

            // assert
            eventContext.ShouldContainEvent<InterviewerAssigned>();
            eventContext.ShouldNotContainEvent<SupervisorAssigned>();
            eventContext.ShouldNotContainEvent<InterviewStatusChanged>();
        }

        [Test]
        public void Interview_in_status_Completed_And_interview_being_reassigned_to_different_responsible_in_another_team_As_result_interviewer_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.Completed));
            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId2, interviewerId: interviewerId2));

            // assert
            eventContext.ShouldContainEvent<SupervisorAssigned>();
            eventContext.ShouldContainEvent<InterviewerAssigned>();
            eventContext.ShouldNotContainEvent<InterviewStatusChanged>();
        }

        [Test]
        public void Interview_in_status_Completed_And_interview_being_reassigned_to_another_supervisor_As_result_supervisor_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.Completed));
            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId2, interviewerId: null));

            // assert
            eventContext.ShouldContainEvent<SupervisorAssigned>();
            eventContext.ShouldContainEvent<InterviewerAssigned>(x => x.InterviewerId == null);
            eventContext.ShouldNotContainEvent<InterviewStatusChanged>();
        }

        [Test]
        public void When_Interview_in_status_InterviewerAssigned_And_interview_being_moved_to_interviewer_in_other_team_As_result_supervisor_and_interviewer_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));
            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId2, interviewerId: interviewerId2));

            // assert
            eventContext.ShouldContainEvent<SupervisorAssigned>();
            eventContext.ShouldContainEvent<InterviewerAssigned>();
            eventContext.ShouldContainEvent<InterviewStatusChanged>(e => e.Status == InterviewStatus.InterviewerAssigned);
        }

        [Test]
        public void When_Interview_in_status_InterviewerAssigned_And_interview_being_moved_to_supervisor_in_same_team_As_result_status_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));
            SetupEventContext();

            // act
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId, interviewerId: null));

            // assert
            eventContext.ShouldContainEvent<SupervisorAssigned>();
            eventContext.ShouldContainEvent<InterviewerAssigned>();
            eventContext.ShouldContainEvent<InterviewStatusChanged>(e => e.Status == InterviewStatus.SupervisorAssigned);
        }

        [Test]
        public void When_Interview_in_status_SupervisorAssigned_And_interview_being_moved_to_the_same_supervisor_As_result_exception_should_be_thrown()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));
            SetupEventContext();

            // act
            void AssignResponsible () => 
                interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId, interviewerId: null));

            // assert
            Assert.Throws(Is.TypeOf<InterviewException>().And.Message.EqualTo($"Interview has assigned on this supervisor already"), AssignResponsible);
        }

        [Test]
        public void When_Interview_in_status_RejectedByHQ_And_interview_assigned_to_interviewer_As_result_status_should_be_RejectedBySupervisor()
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
            interview.AssignResponsible(Create.Command.AssignResponsibleCommand(supervisorId: supervisorId, interviewerId: interviewerId));

            // assert
            eventContext.ShouldContainEvent<InterviewRejected>();
            eventContext.ShouldContainEvent<InterviewStatusChanged>(e => e.Status == InterviewStatus.RejectedBySupervisor);
            eventContext.ShouldContainEvent<InterviewerAssigned>();
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
