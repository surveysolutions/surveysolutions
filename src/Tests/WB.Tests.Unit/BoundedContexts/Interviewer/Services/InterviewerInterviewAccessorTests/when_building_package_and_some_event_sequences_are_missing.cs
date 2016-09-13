using System;
using System.Collections.Generic;
using Machine.Specifications;
using Ncqrs.Eventing;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_building_package_and_some_event_sequences_are_missing
    {
        Establish context = () =>
        {
            var events = new List<CommittedEvent>
            {
                Create.Other.CommittedEvent(eventSequence: 1),
                Create.Other.CommittedEvent(eventSequence: 2),
                Create.Other.CommittedEvent(eventSequence: 4),
                Create.Other.CommittedEvent(eventSequence: 5),
            };

            interviewAccessor = Create.Service.InterviewerInterviewAccessor(
                interviewViewRepository: Stub<IPlainStorage<InterviewView>>.Returning(Create.Entity.InterviewView()),
                eventStore: Stub<IInterviewerEventStorage>.Returning<IEnumerable<CommittedEvent>>(events));
        };

        Because of = () =>
            exception = Catch.Exception(() => interviewAccessor.GetInteviewEventsPackageOrNullAsync(interviewId).WaitAndUnwrapException());

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_exception_with_message_containing_missing_event_sequence = () =>
            exception.Message.ShouldContain("3");

        It should_throw_exception_with_message_containing__event____sequence____missing__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("event", "sequence", "missing");

        private static Exception exception;
        private static InterviewerInterviewAccessor interviewAccessor;
        private static Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
