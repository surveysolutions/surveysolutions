using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_is_created : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewStatusesStorage = new TestInMemoryWriter<InterviewSummary>();
            statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewOnClientCreated(interviewId: Guid.NewGuid()));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: Guid.NewGuid()));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewFromPreloadedDataCreated(interviewId: Guid.NewGuid()));

            denormalizer = CreateDenormalizer(interviewStatuses: interviewStatusesStorage);
            BecauseOf();
        }

        private void BecauseOf()
            {
                foreach (var publishableEvent in statusEventsToPublish)
                {
                    denormalizer.Handle(new[] {publishableEvent}, publishableEvent.EventSourceId);
                }
            }

        [NUnit.Framework.Test] public void should_create_InterviewStatuses_for_3_interviews () => statusEventsToPublish.TrueForAll(s => interviewStatusesStorage.GetById(s.EventSourceId.FormatGuid())!=null).Should().BeTrue();

        private static InterviewSummaryCompositeDenormalizer denormalizer;
        private static List<IPublishableEvent> statusEventsToPublish;
        private static TestInMemoryWriter<InterviewSummary> interviewStatusesStorage;
    }
}
