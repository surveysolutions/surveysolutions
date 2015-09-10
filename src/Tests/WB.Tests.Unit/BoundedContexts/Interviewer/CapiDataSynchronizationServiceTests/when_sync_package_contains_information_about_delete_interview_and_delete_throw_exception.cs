using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_interview_and_delete_throw_exception : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            syncItem = new InterviewSyncPackageDto
                       {
                           Content = interviewId.ToString(), 
                           MetaInfo = "some metadata", 
                       };

            commandService = new Mock<ICommandService>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock = new Mock<ICapiCleanUpService>();
            cleanUpExecutorMock.Setup(x => x.DeleteInterview(interviewId)).Throws<NullReferenceException>();

            var viewFactory = new Mock<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>();
            var existingInterview = new InterviewMetaInfo
            {
                ResponsibleId = responsibleId
            };
            viewFactory.SetReturnsDefault(existingInterview);

            var userIdentity = Mock.Of<IUserIdentity>(x=>x.UserId == responsibleId);
            var principal = Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == userIdentity);

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, 
                commandService.Object, null, null, null, cleanUpExecutorMock.Object,
                interviewMetaInfoFactory: viewFactory.Object, principal: principal);
        };

        Because of = () => exception = Catch.Exception(() => capiDataSynchronizationService.ProcessDownloadedInterviewPackages(syncItem, SyncItemType.DeleteInterview));

        It should_never_call_any_command =
            () => commandService.Verify(x => x.Execute(Moq.It.IsAny<ICommand>(), null), Times.Never);

        It should_cleanup_data_for_interview =
            () => cleanUpExecutorMock.Verify(x => x.DeleteInterview(interviewId), Times.Once);

        It should_not_throw_Exception = () => 
            exception.ShouldBeNull();

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static InterviewSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        private static Guid interviewId;
        private static Exception exception;
        private static Guid responsibleId;
    }
}
