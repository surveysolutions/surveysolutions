using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_status_changed_on_RejectedBySupervisor : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedInterview =  Create.InterviewData(
                status: InterviewStatus.RejectedBySupervisor,
                interviewId: interviewId,
                responsibleId: userId);

            var synchronizationDto = CreateSynchronizationDto(interviewId);

            var interviews = Mock.Of<IReadSideKeyValueStorage<InterviewData>>(x => x.GetById(interviewId.FormatGuid()) == storedInterview);

            synchronizationDtoFactory = Mock.Of<IInterviewSynchronizationDtoFactory>(
                x => x.BuildFrom(storedInterview, userId, InterviewStatus.RejectedBySupervisor, comments) == synchronizationDto);
            

            interviewSyncPackageContentMock = new Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>>();

            denormalizer = CreateDenormalizer(
                interviews : interviews,
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object,
                synchronizationDtoFactory: synchronizationDtoFactory);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.InterviewStatusChanged(interviewId, InterviewStatus.RejectedBySupervisor,comments));

        It should_create_interview_package = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(
                Moq.It.IsAny<InterviewSyncPackageContent>(),
                Moq.It.Is<InterviewSyncPackageMeta>(i => i.InterviewId == interviewId && i.ItemType == SyncItemType.Interview),
                partialPackageId,
                CounterId), Times.Once);

        private static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>> interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>>();

        private static Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>> interviewSyncPackageContentMock;
        private static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static readonly Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static IInterviewSynchronizationDtoFactory synchronizationDtoFactory;
        private static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static string comments = "comment";
    }
}