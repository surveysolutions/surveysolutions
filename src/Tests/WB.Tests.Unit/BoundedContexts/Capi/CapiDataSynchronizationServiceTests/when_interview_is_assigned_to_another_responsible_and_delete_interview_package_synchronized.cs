using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_interview_is_assigned_to_another_responsible_and_delete_interview_package_synchronized : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();

            syncItem = new InterviewSyncPackageDto
            {
                Content = interviewId.ToString(),
                MetaInfo = "some metadata"
            };

            var viewFactory = new Mock<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>();
            var existingInterview = new InterviewMetaInfo
            {
                ResponsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
            };

            viewFactory.SetReturnsDefault(existingInterview);

            cleanUpExecutorMock = new Mock<ICapiCleanUpService>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(
                capiCleanUpService: cleanUpExecutorMock.Object,
                interviewMetaInfoFactory: viewFactory.Object);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.DeleteInterview, Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"));

        It should_not_cleanup_data_for_another_user =
            () => cleanUpExecutorMock.Verify(x => x.DeleteInterview(interviewId), Times.Never);

        static CapiDataSynchronizationService capiDataSynchronizationService;
        static InterviewSyncPackageDto syncItem;
        static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        static Guid interviewId;
    }
}

