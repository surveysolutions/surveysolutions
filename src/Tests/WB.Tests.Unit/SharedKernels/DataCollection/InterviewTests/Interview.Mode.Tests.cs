using System;
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
    public class InterviewModeTests : InterviewTestsContext
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
        public void When_Interview_mode_is_changed()
        {
            // arrange
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(Create.Event.InterviewerAssigned(supervisorId, interviewerId));
            SetupEventContext();

            // act
            interview.ChangeInterviewMode(headquarterId, DateTimeOffset.Now, InterviewMode.CAWI);

            // assert
            eventContext.ShouldContainEvent<InterviewModeChanged>(x => x.Mode == InterviewMode.CAWI);
        }

        
        [Test]
        public void should_not_allow_command_if_deleted()
        {
            var interview = SetupInterview();
            interview.Apply(Create.Event.SupervisorAssigned(supervisorId, supervisorId));
            interview.Delete(supervisorId, DateTimeOffset.Now);

            // Act
            TestDelegate act = () => interview.ChangeInterviewMode(headquarterId, DateTimeOffset.Now, InterviewMode.CAWI);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>());
        }


        [TestCase(InterviewStatus.Created)]
        [TestCase(InterviewStatus.SupervisorAssigned)]
        [TestCase(InterviewStatus.InterviewerAssigned)]
        [TestCase(InterviewStatus.Restarted)]
        [TestCase(InterviewStatus.RejectedBySupervisor)]
        public void should_allow_command_in_status(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.ChangeInterviewMode(headquarterId, DateTimeOffset.Now, InterviewMode.CAWI);

            // Assert
            Assert.That(act, Throws.Nothing);
        }

        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.Completed)]
        public void should_not_allow_command_in_statuses(InterviewStatus targetStatus)
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            interview.Apply(Create.Event.InterviewStatusChanged(targetStatus));

            // Act
            TestDelegate act = () => interview.ChangeInterviewMode(headquarterId, DateTimeOffset.Now, InterviewMode.CAWI);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>());
        }

    }
}
