using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(SupervisorInterviewStateCalculationStrategy))]
    public class SupervisorInterviewStateCalculationStrategyTests
    {
        [Test]
        public void should_mark_interview_as_invalid_when_invalid_supervisor_question_answered()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountInvalidEntitiesInInterviewForSupervisor() == 1);

            // Act
            SimpleGroupStatus status = GetService().CalculateSimpleStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(SimpleGroupStatus.Invalid));
        }

        [Test]
        public void should_mark_interview_as_completed_when_all_questions_answered()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 1);

            // Act
            SimpleGroupStatus status = GetService().CalculateSimpleStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(SimpleGroupStatus.Completed));
        }

        [Test]
        public void should_mark_intrerview_as_other_when_interview_is_not_fully_answered()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 2 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 1);

            // Act
            SimpleGroupStatus status = GetService().CalculateSimpleStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(SimpleGroupStatus.Other));
        }

        [Test]
        public void should_mark_interview_as_CompletedInvalid_when_all_questions_answered()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountInvalidEntitiesInInterviewForSupervisor() == 1);

            // Act
            GroupStatus status = GetService().CalculateDetailedStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.CompletedInvalid));
        }

        [Test]
        public void should_mark_interview_as_Completed_when_all_questions_answered()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountInvalidEntitiesInInterviewForSupervisor() == 0);

            // Act
            GroupStatus status = GetService().CalculateDetailedStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void should_mark_interview_as_started_when_there_is_at_least_one_answered_question()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 2 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 1 &&
                                                             x.CountInvalidEntitiesInInterviewForSupervisor() == 0);

            // Act
            GroupStatus status = GetService().CalculateDetailedStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void should_mark_interview_as_not_started_when_there_is_no_answered_question()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.CountActiveQuestionsInInterviewForSupervisor() == 2 &&
                                                             x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 0);

            // Act
            GroupStatus status = GetService().CalculateDetailedStatus(interview);

            // Assert
            Assert.That(status, Is.EqualTo(GroupStatus.NotStarted));
        }

        IInterviewStateCalculationStrategy GetService()
        {
            return new SupervisorInterviewStateCalculationStrategy();
        }
    }
}
