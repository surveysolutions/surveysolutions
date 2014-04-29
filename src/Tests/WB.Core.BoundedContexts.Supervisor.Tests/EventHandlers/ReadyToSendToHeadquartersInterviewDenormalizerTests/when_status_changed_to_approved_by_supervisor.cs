using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.ReadyToSendToHeadquartersInterviewDenormalizerTests
{
    internal class when_status_changed_to_approved_by_supervisor : ReadyToSendToHeadquartersInterviewDenormalizerTestsContext
    {
        Establish context = () =>
        {
            status = InterviewStatus.ApprovedBySupervisor;

            denormalizer = CreateReadyToSendToHeadquartersInterviewDenormalizer();
        };

        Because of = () =>
            result = denormalizer.Update(null, ToPublishedEvent(new InterviewStatusChanged(status, null), interviewId));

        It should_return_not_null = () =>
            result.ShouldNotBeNull();

        It should_return_interview_id_equal_to_one_specified_in_event = () =>
            result.InterviewId.ShouldEqual(interviewId);

        private static ReadyToSendToHeadquartersInterview result;
        private static ReadyToSendToHeadquartersInterviewDenormalizer denormalizer;
        private static InterviewStatus status;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}