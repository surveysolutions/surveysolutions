using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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

        It should_save_responsible_role_as_Operator = () => viewState.ResponsibleRole.ShouldEqual(UserRoles.Interviewer);

        It should_reset_ReceivedByInterviewer_flag = () => viewState.ReceivedByInterviewer.ShouldBeFalse();


        static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        static InterviewData viewState;
        static Guid interviewerIdOld = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid interviewerIdNew = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static IPublishedEvent<InterviewerAssigned> interviewerAssignedEvent;
    }
}