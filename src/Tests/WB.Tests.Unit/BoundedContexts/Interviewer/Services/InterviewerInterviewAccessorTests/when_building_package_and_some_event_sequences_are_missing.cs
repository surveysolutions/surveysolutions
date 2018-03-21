using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_building_package_and_some_event_sequences_are_missing
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
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
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = Assert.Throws<ArgumentException>(() => interviewAccessor.GetInteviewEventsPackageOrNull(interviewId));

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing_missing_event_sequence () =>
            exception.Message.Should().Contain("3");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__event____sequence____missing__ () =>
            exception.Message.ToLower().ToSeparateWords().Should().Contain("event", "sequence", "missing");

        private static Exception exception;
        private static InterviewerInterviewAccessor interviewAccessor;
        private static Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
