using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable;
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

            denormalizer = CreateDenormalizer(
                interviewSummarys: interviews,
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.InterviewStatusChanged(interviewId, InterviewStatus.Deleted, comments, eventId: Guid.Parse(partialPackageId)));

        It should_create_interview_package = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(
                Moq.It.Is<InterviewSyncPackageMeta>(i => i.InterviewId == interviewId && i.ItemType == SyncItemType.DeleteInterview), 
                partialPackageId), 
                Times.Once);

        static InterviewSynchronizationDenormalizer denormalizer;
        static Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>> interviewPackageStorageWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>();
        
        static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        static string comments = "comment";
    }
}