using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_created_on_client_and_rejected_by_supervisor_is_assigned_to_interviewer : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            InterviewData data = Create.InterviewData(createdOnClient: true, 
                status: InterviewStatus.RejectedBySupervisor,
                interviewId: interviewId);

            var interviews = new Mock<IReadSideKeyValueStorage<InterviewData>>();
            interviews.SetReturnsDefault(data);

            var synchronizationDto = CreateSynchronizationDto(interviewId);

            var synchronizationDtoFactory = Mock.Of<IInterviewSynchronizationDtoFactory>(
                    x => x.BuildFrom(data, Moq.It.IsAny<Guid>(), InterviewStatus.InterviewerAssigned, Moq.It.IsAny<string>()) == synchronizationDto);
            
            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            var interviewSummary = new InterviewSummary
            {
                WasCreatedOnClient = true,
                Status = InterviewStatus.RejectedBySupervisor,
                WasRejectedBySupervisor = true
            };

            interviewSummaryWriterMock.SetReturnsDefault(interviewSummary);

            synchronizationDenormalizer = CreateDenormalizer(
                interviews: interviews.Object,
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object, 
                interviewSummarys: interviewSummaryWriterMock.Object,
                synchronizationDtoFactory: synchronizationDtoFactory);
        };

        Because of = () => synchronizationDenormalizer.Handle(Create.InterviewerAssignedEvent());

        It should_store_meta_information_of_package = () =>
            interviewPackageStorageWriterMock.Verify(x => 
                x.Store(
                Moq.It.IsAny<InterviewSyncPackageContent>(),
                Moq.It.IsAny<InterviewSyncPackageMeta>(), 
                Moq.It.IsAny<string>(),
                CounterId), Times.Once);

        static InterviewSynchronizationDenormalizer synchronizationDenormalizer;
        static Guid interviewId;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>> interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>>();
    }
}

