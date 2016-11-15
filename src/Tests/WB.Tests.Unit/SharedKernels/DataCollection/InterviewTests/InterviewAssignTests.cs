using System;
using System.Diagnostics;
using System.Linq;
using Machine.Specifications;
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
        [Test]
        public void When_Interview_in_status_SupervisorAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed()
        {
            // arrange
            var supervisorId    = Guid.Parse("11111111111111111111111111111111");
            var supervisorId2   = Guid.Parse("22222222222222222222222222222222");
            var questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));

            var eventContext = Create.Other.EventContext();

            // act
            var exception = Catch.Exception(() => interview.AssignSupervisor(supervisorId, supervisorId2));

            // assert
            Assert.That(exception, Is.Null);
            eventContext.AssertIfDoesntContainEvent<SupervisorAssigned>();
            eventContext.AssertIfContainEvent<InterviewStatusChanged>();
            eventContext.AssertIfContainEvent<InterviewerAssigned>();

            eventContext.Dispose();
        }


        [Test]
        public void Interview_in_status_InterviewerAssigned_And_interview_being_reassigned_within_one_team_As_result_interviewer_should_be_changed()
        {
            // arrange
            var interviewerId   = Guid.Parse("99999999999999999999999999999999");
            var interviewerId2  = Guid.Parse("22222222222222222222222222222222");
            var supervisorId    = Guid.Parse("11111111111111111111111111111111");
            var questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

            var eventContext = Create.Other.EventContext();

            // act
            var exception = Catch.Exception(() => interview.AssignInterviewer(supervisorId, interviewerId2, DateTime.UtcNow));

            // assert
            Assert.That(exception, Is.Null);
            eventContext.AssertIfDoesntContainEvent<InterviewerAssigned>();
            eventContext.AssertIfContainEvent<SupervisorAssigned>();
            eventContext.AssertIfContainEvent<InterviewStatusChanged>();

            eventContext.Dispose();
        }

        [Test]
        public void Interview_in_status_RejectedBySupervisor_And_interview_being_reassigned_within_one_team_As_result_interviewer_should_be_changed_Status_should_be_intact()
        {
            // arrange
            var interviewerId   = Guid.Parse("99999999999999999999999999999999");
            var supervisorId    = Guid.Parse("11111111111111111111111111111111");
            var headquarterId   = Guid.Parse("77777777777777777777777777777777");
            var questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewApproved(supervisorId));
            interview.Apply(Create.Event.InterviewRejectedByHQ(headquarterId));
            interview.Apply(Create.Event.InterviewRejected(supervisorId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.RejectedBySupervisor));

            var eventContext = Create.Other.EventContext();

            // act
            var exception = Catch.Exception(() => interview.AssignInterviewer(supervisorId, interviewerId, DateTime.UtcNow));

            // assert
            Assert.That(exception, Is.Null);
            eventContext.AssertIfDoesntContainEvent<InterviewerAssigned>();
            eventContext.AssertIfContainEvent<SupervisorAssigned>();
            eventContext.AssertIfContainEvent<InterviewStatusChanged>();

            eventContext.Dispose();
        }


        [Test]
        public void Interview_in_status_Completed_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null()
        {
            // arrange
            var interviewerId   = Guid.Parse("99999999999999999999999999999999");
            var supervisorId2   = Guid.Parse("22222222222222222222222222222222");
            var supervisorId    = Guid.Parse("11111111111111111111111111111111");
            var headquarterId   = Guid.Parse("77777777777777777777777777777777");
            var questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewApproved(supervisorId));
            interview.Apply(Create.Event.InterviewRejectedByHQ(headquarterId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters));

            var eventContext = Create.Other.EventContext();

            // act
            var exception = Catch.Exception(() => interview.AssignSupervisor(headquarterId, supervisorId2));

            // assert
            Assert.That(exception, Is.Null);
            eventContext.AssertIfDoesntContainEvent<SupervisorAssigned>();
            eventContext.AssertIfContainEvent<InterviewerAssigned>();
            eventContext.AssertIfContainEvent<InterviewStatusChanged>();

            eventContext.Dispose();
        }


        [Test]
        public void Interview_in_status_RejectedByHQ_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed_and_interviewer_set_to_null()
        {
            // arrange
            var interviewerId   = Guid.Parse("99999999999999999999999999999999");
            var supervisorId2   = Guid.Parse("22222222222222222222222222222222");
            var supervisorId    = Guid.Parse("11111111111111111111111111111111");
            var headquarterId   = Guid.Parse("77777777777777777777777777777777");
            var questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId, DateTime.UtcNow.AddHours(-1)));
            interview.Apply(Create.Event.InteviewCompleted());
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.Completed));

            var eventContext = Create.Other.EventContext();

            // act
            var exception = Catch.Exception(() => interview.AssignSupervisor(headquarterId, supervisorId2));

            // assert
            Assert.That(exception, Is.Null);
            eventContext.AssertIfDoesntContainEvent<SupervisorAssigned>();
            eventContext.AssertIfContainEvent<InterviewerAssigned>();
            eventContext.AssertIfContainEvent<InterviewStatusChanged>();

            eventContext.Dispose();
        }



    }

    internal static class EventContextExtension
    {
        [DebuggerStepThrough]
        public static void AssertIfDoesntContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            bool isExists = condition == null
                ? eventContext.Events.Any(@event => @event.Payload is TEvent)
                : eventContext.Events.Any(@event => @event.Payload is TEvent && condition.Invoke((TEvent)@event.Payload));
            Assert.That<bool>(isExists, Is.True);
        }

        [DebuggerStepThrough]
        public static void AssertIfContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            bool isExists = condition == null
                ? eventContext.Events.Any(@event => @event.Payload is TEvent)
                : eventContext.Events.Any(@event => @event.Payload is TEvent && condition.Invoke((TEvent)@event.Payload));
            Assert.That<bool>(isExists, Is.False);
        }
    }
}