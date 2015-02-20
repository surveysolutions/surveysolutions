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
    internal class when_interview_status_changed_on_Deleted : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedInterview = Create.InterviewSummary();

            var interviews = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(x => x.GetById(interviewId.FormatGuid()) == storedInterview);

            interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>>();
            interviewPackageStorageWriterMock.Setup(x => x.GetNextOrder()).Returns(sortIndex);

            interviewSyncPackageContentMock = new Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>>();

            denormalizer = CreateDenormalizer(
                interviewSummarys: interviews,
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object,
                interviewSyncPackageContentStorage: interviewSyncPackageContentMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.InterviewStatusChanged(interviewId, InterviewStatus.Deleted, comments));

        It should_create_interview_package = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(Moq.It.Is<InterviewSyncPackageMetaInformation>(
                i => i.InterviewId == interviewId
                     && i.ItemType == SyncItemType.DeleteInterview
                     && i.PackageId == packageId
                     && i.SortIndex == sortIndex), packageId), Times.Once);

        It should_store_package_content_once = () =>
            interviewSyncPackageContentMock.Verify(x => x.Store(Moq.It.IsAny<InterviewSyncPackageContent>(), packageId), Times.Once);

        private static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>> interviewPackageStorageWriterMock;
        private static Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>> interviewSyncPackageContentMock;
        private static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static int sortIndex = 5;
        private static string packageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$5";
        private static string comments = "comment";
    }
}