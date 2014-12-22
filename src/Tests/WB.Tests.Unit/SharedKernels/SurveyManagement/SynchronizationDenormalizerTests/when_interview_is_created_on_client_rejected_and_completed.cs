using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_is_created_on_client_rejected_and_completed : SynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            syncStorage = new Mock<ISynchronizationDataStorage>();

            InterviewData data = Create.InterviewData(createdOnClient: true, 
                status: InterviewStatus.Completed,
                interviewId: interviewId);

            var interviews = new Mock<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>();
            interviews.SetReturnsDefault(new ViewWithSequence<InterviewData>(data, 1));

            synchronizationDenormalizer = CreateDenormalizer(interviews: interviews.Object, 
                synchronizationDataStorage: syncStorage.Object);
        };

        Because of = () =>
        {
            synchronizationDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.RejectedBySupervisor, interviewId: interviewId));
            synchronizationDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed, interviewId: interviewId));
        };

        It should_create_deletion_synchronization_package = () => 
            syncStorage.Verify(x => x.MarkInterviewForClientDeleting(interviewId, Moq.It.IsAny<Guid?>(), Moq.It.IsAny<DateTime>()));

        static SynchronizationDenormalizer synchronizationDenormalizer;
        static Mock<ISynchronizationDataStorage> syncStorage;
        static Guid interviewId;
    }
}

