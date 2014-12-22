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
    internal class when_interview_created_on_client_and_assigned_to_interviewer : SynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            InterviewData data = Create.InterviewData(createdOnClient: true);

            var interviews = new Mock<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>();
            interviews.SetReturnsDefault(new ViewWithSequence<InterviewData>(data, 1));

            syncStorage = new Mock<ISynchronizationDataStorage>();

            denormalizer = CreateDenormalizer(interviews.Object, syncStorage.Object);
        };


        Because of = () => denormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed));

        It should_not_send_deletion_package_to_tablets = () =>
            syncStorage.Verify(x => x.MarkInterviewForClientDeleting(interviewId, Moq.It.IsAny<Guid?>(), Moq.It.IsAny<DateTime>()), Times.Never);

        static SynchronizationDenormalizer denormalizer;
        static Mock<ISynchronizationDataStorage> syncStorage;
        static Guid interviewId;
    }
}

