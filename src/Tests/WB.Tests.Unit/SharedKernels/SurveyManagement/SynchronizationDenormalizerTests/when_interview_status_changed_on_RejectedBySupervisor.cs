using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
                x => x.BuildFrom(storedInterview, userId, InterviewStatus.RejectedBySupervisor, comments, Moq.It.IsAny<DateTime?>(), Moq.It.IsAny<DateTime?>()) == synchronizationDto);

            var serializer = Mock.Of<ISerializer>(x=>x.Serialize(Moq.It.IsAny<object>(), Moq.It.IsAny<TypeSerializationSettings>()) == String.Empty);
            
            denormalizer = CreateDenormalizer(
                interviews : interviews,
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object,
                synchronizationDtoFactory: synchronizationDtoFactory,
                serializer: serializer);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.Published.InterviewStatusChanged(interviewId, InterviewStatus.RejectedBySupervisor, comments, eventId: Guid.Parse(partialPackageId)));

        It should_create_interview_package = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(
                Moq.It.Is<InterviewSyncPackageMeta>(i => i.InterviewId == interviewId && i.ItemType == SyncItemType.Interview),
                Moq.It.Is<string>(id => id.StartsWith(partialPackageId))), Times.Once);

        static InterviewSynchronizationDenormalizer denormalizer;
        static Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>> interviewPackageStorageWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>();

        static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid userId = Guid.Parse("11111111111111111111111111111111");
        static IInterviewSynchronizationDtoFactory synchronizationDtoFactory;
        static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        static string comments = "comment";
    }
}