﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_interview_is_assigned_to_interviewer_in_RejectedByHQ_status : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            viewState = CreateViewWithSequenceOfInterviewData();
            synchronizationDataStorage = new Mock<ISynchronizationDataStorage>();
            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional();
            viewState.Status = InterviewStatus.RejectedByHeadquarters;
        };

       Because of = () => interviewEventHandlerFunctional.Update(viewState, CreatePublishableEvent(new InterviewerAssigned(Guid.NewGuid(), Guid.NewGuid())));

       It should_not_sent_it_to_CAPI = () => synchronizationDataStorage.Verify(x => x.SaveInterview(Moq.It.IsAny<InterviewSynchronizationDto>(), Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Never);
       
       static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
       static InterviewData viewState;
       static Mock<ISynchronizationDataStorage> synchronizationDataStorage;
    }
}

