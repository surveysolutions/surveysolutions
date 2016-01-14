using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
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
    internal class when_interview_is_reassigned_to_interviewer : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            viewState = CreateViewWithSequenceOfInterviewData();
            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional();
            viewState.Status = InterviewStatus.InterviewerAssigned;
            viewState.ReceivedByInterviewer = true;
            viewState.InterviewId = interviewerIdOld;

            interviewerAssignedEvent = CreatePublishableEvent(new InterviewerAssigned(Guid.NewGuid(), interviewerIdNew, DateTime.Now));
        };

        Because of = () => interviewEventHandlerFunctional.Update(viewState, interviewerAssignedEvent);


        It should_change_responsible_to_new_interviewer = () => viewState.ResponsibleId.ShouldEqual(interviewerIdNew);

        It should_save_responsible_role_as_Operator = () => viewState.ResponsibleRole.ShouldEqual(UserRoles.Operator);

        It should_reset_ReceivedByInterviewer_flag = () => viewState.ReceivedByInterviewer.ShouldBeFalse();


        static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        static InterviewData viewState;
        static Guid interviewerIdOld = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid interviewerIdNew = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static IPublishedEvent<InterviewerAssigned> interviewerAssignedEvent;
    }
}