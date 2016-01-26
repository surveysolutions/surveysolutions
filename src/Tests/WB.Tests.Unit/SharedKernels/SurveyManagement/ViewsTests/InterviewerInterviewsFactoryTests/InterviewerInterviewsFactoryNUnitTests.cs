using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewerInterviewsFactoryTests
{
    [TestFixture]
    internal class InterviewerInterviewsFactoryNUnitTests
    {
        [Test]
        public void GetInterviewDetails_When_interview_in_interviewer_assigned_state_then_lastRejectedBySupervisorStatus_should_be_null()
        {
            var interviewId = Guid.NewGuid();
            var interviewSynchronizationDtoFactoryMock=new Mock<IInterviewSynchronizationDtoFactory>();
            var interviewerInterviewsFactory =
                this.CreateInterviewerInterviewsFactory(
                    synchronizationDtoFactory: interviewSynchronizationDtoFactoryMock.Object,
                    statusHistory: new[] {Create.CommentedStatusHistroyView()});

            interviewerInterviewsFactory.GetInterviewDetails(interviewId);

            interviewSynchronizationDtoFactoryMock.Verify(
                x =>
                    x.BuildFrom(Moq.It.IsAny<InterviewData>(), Moq.It.IsAny<Guid>(), InterviewStatus.InterviewerAssigned,
                        null, null, Moq.It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void GetInterviewDetails_When_interview_in_rejected_by_supervisor_state_with_comment_then_lastRejectedBySupervisorStatus_should_not_be_null_and_comment_should_be_preserved()
        {
            var interviewId = Guid.NewGuid();
            var interviewSynchronizationDtoFactoryMock = new Mock<IInterviewSynchronizationDtoFactory>();
            var interviewerInterviewsFactory =
                this.CreateInterviewerInterviewsFactory(
                    synchronizationDtoFactory: interviewSynchronizationDtoFactoryMock.Object,
                    statusHistory:
                        new[]
                        {
                            Create.CommentedStatusHistroyView(),
                            Create.CommentedStatusHistroyView(InterviewStatus.Completed),
                            Create.CommentedStatusHistroyView(InterviewStatus.RejectedBySupervisor, "comment")
                        });

            interviewerInterviewsFactory.GetInterviewDetails(interviewId);

            interviewSynchronizationDtoFactoryMock.Verify(
                x =>
                    x.BuildFrom(Moq.It.IsAny<InterviewData>(), Moq.It.IsAny<Guid>(), InterviewStatus.RejectedBySupervisor,
                        "comment", Moq.It.IsAny<DateTime>(), Moq.It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void GetInterviewDetails_When_interview_in_interviewer_assigned_state_but_was_rejected_by_supervisor_with_comment_before_then_lastRejectedBySupervisorStatus_should_not_be_null_and_comment_should_be_preserved()
        {
            var interviewId = Guid.NewGuid();
            var interviewSynchronizationDtoFactoryMock = new Mock<IInterviewSynchronizationDtoFactory>();
            var interviewerInterviewsFactory =
                this.CreateInterviewerInterviewsFactory(
                    synchronizationDtoFactory: interviewSynchronizationDtoFactoryMock.Object,
                    statusHistory:
                        new[]
                        {
                            Create.CommentedStatusHistroyView(),
                            Create.CommentedStatusHistroyView(InterviewStatus.Completed),
                            Create.CommentedStatusHistroyView(InterviewStatus.RejectedBySupervisor, "comment"),
                            Create.CommentedStatusHistroyView()
                        });

            interviewerInterviewsFactory.GetInterviewDetails(interviewId);

            interviewSynchronizationDtoFactoryMock.Verify(
                x =>
                    x.BuildFrom(Moq.It.IsAny<InterviewData>(), Moq.It.IsAny<Guid>(), InterviewStatus.InterviewerAssigned,
                        "comment", Moq.It.IsAny<DateTime>(), Moq.It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void
            GetInterviewDetails_When_interview_in_rejected_by_supervisor_state_without_comment_for_second_then_lastRejectedBySupervisorStatus_should_not_be_null_and_comment_should_be_empty
            ()
        {
            var interviewId = Guid.NewGuid();
            var interviewSynchronizationDtoFactoryMock = new Mock<IInterviewSynchronizationDtoFactory>();
            var interviewerInterviewsFactory =
                this.CreateInterviewerInterviewsFactory(
                    synchronizationDtoFactory: interviewSynchronizationDtoFactoryMock.Object,
                    statusHistory:
                        new[]
                        {
                            Create.CommentedStatusHistroyView(),
                            Create.CommentedStatusHistroyView(InterviewStatus.Completed),
                            Create.CommentedStatusHistroyView(InterviewStatus.RejectedBySupervisor, "comment"),
                            Create.CommentedStatusHistroyView(InterviewStatus.Completed),
                            Create.CommentedStatusHistroyView(InterviewStatus.RejectedBySupervisor)
                        });

            interviewerInterviewsFactory.GetInterviewDetails(interviewId);

            interviewSynchronizationDtoFactoryMock.Verify(
                x =>
                    x.BuildFrom(Moq.It.IsAny<InterviewData>(), Moq.It.IsAny<Guid>(),
                        InterviewStatus.RejectedBySupervisor,
                        null, Moq.It.IsAny<DateTime>(), Moq.It.IsAny<DateTime>()), Times.Once);
        }

        private InterviewerInterviewsFactory CreateInterviewerInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader=null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory = null,
            InterviewData interviewData = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null,
            params CommentedStatusHistroyView[] statusHistory)
        {
            interviewData = interviewData ?? Create.InterviewData();
            var changeStatusView=new ChangeStatusView() {StatusHistory = statusHistory.ToList()};
            return
                new InterviewerInterviewsFactory(
                    reader ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                    questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                    synchronizationDtoFactory ?? Mock.Of<IInterviewSynchronizationDtoFactory>(),
                    Mock.Of<IReadSideKeyValueStorage<InterviewData>>(_=>_.GetById(Moq.It.IsAny<string>())==interviewData),
                    Mock.Of<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>(_=>_.Load(Moq.It.IsAny<ChangeStatusInputModel>())== changeStatusView),
                    incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>());
        }
    }
}