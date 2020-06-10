using System;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(Interview))]
    public class InterviewHqRejectTests : InterviewTestsContext
    {
        EventContext eventContext;

        readonly Guid interviewerId = Guid.Parse("99999999999999999999999999999999");
        readonly Guid interviewerId2 = Guid.Parse("22222222222222222222222222222222");
        readonly Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        readonly Guid supervisorId2 = Guid.Parse("88888888888888888888888888888888");
        readonly Guid headquarterId = Guid.Parse("77777777777777777777777777777777");
        readonly Guid questionnaireId = Guid.Parse("33333333333333333333333333333333");

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
        public void When_Interview_in_status_ApprovedBySupervisor_And_interview_rejected_by_hq_on_interviewer_as_result_supervisor_and_interviewer_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor));
            SetupEventContext();

            // act
            interview.HqRejectInterviewToInterviewer(headquarterId, interviewerId2, supervisorId2, "comment", DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<InterviewRejectedByHQ>();
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewerAssigned>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>();
        }


        [Test]
        public void When_Interview_in_status_ApprovedBySupervisor_And_interview_rejected_by_hq_on_supervisor_as_result_supervisor_should_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor));
            SetupEventContext();

            // act
            interview.HqRejectInterviewToSupervisor(headquarterId, supervisorId2, "comment", DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<InterviewRejectedByHQ>();
            eventContext.AssertThatContainsEvent<SupervisorAssigned>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>();
        }

        [Test]
        public void When_Interview_in_status_ApprovedBySupervisor_And_interview_rejected_by_hq_on_the_same_supervisor_as_result_supervisor_should_not_be_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId));
            interview.Apply(Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor));
            SetupEventContext();

            // act
            interview.HqRejectInterviewToSupervisor(headquarterId, supervisorId, "comment", DateTimeOffset.Now);

            // assert
            eventContext.AssertThatContainsEvent<InterviewRejectedByHQ>();
            eventContext.AssertThatContainsEvent<InterviewStatusChanged>();
            eventContext.AssertThatDoesNotContainEvent<SupervisorAssigned>();
        }
    }
}
