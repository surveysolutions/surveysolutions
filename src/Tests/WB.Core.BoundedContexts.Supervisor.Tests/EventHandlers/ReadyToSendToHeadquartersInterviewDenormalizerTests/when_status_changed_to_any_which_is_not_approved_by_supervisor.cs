using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.ReadyToSendToHeadquartersInterviewDenormalizerTests
{
    internal class when_status_changed_to_any_which_is_not_approved_by_supervisor : ReadyToSendToHeadquartersInterviewDenormalizerTestsContext
    {
        Establish context = () =>
        {
            status = InterviewStatus.ReadyForInterview;

            denormalizer = CreateReadyToSendToHeadquartersInterviewDenormalizer();
        };

        Because of = () =>
            result = denormalizer.Update(null, ToPublishedEvent(new InterviewStatusChanged(status, null)));

        It should_return_null = () =>
            result.ShouldBeNull();

        private static ReadyToSendToHeadquartersInterview result;
        private static ReadyToSendToHeadquartersInterviewDenormalizer denormalizer;
        private static InterviewStatus status;
    }
}