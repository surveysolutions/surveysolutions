using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_InterviewHardDeleted_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            synchronizationDataStorageMock=new Mock<ISynchronizationDataStorage>();
            userId = Guid.Parse("10000000000000000000000000000000");
            viewState = CreateViewWithSequenceOfInterviewData();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(synchronizationDataStorageMock.Object);
            interviewHardDeletedEvent = CreatePublishableEvent(new InterviewHardDeleted(userId));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Delete(viewState,interviewHardDeletedEvent);

        It should_interview_amrked_as_deleted_in_sync_storage_once = () =>
            synchronizationDataStorageMock.Verify(x => x.MarkInterviewForClientDeleting(interviewHardDeletedEvent.EventSourceId,viewState.Document.ResponsibleId, Moq.It.IsAny<DateTime>()), Times.Once);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid userId;
        private static Mock<ISynchronizationDataStorage> synchronizationDataStorageMock;
        private static IPublishedEvent<InterviewHardDeleted> interviewHardDeletedEvent;
    }
}
