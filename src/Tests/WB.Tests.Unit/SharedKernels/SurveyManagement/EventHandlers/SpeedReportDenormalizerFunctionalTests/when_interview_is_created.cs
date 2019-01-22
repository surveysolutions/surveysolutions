using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.SpeedReportDenormalizerFunctionalTests
{
    [TestFixture]
    internal class when_interview_is_created : SpeedReportDenormalizerFunctionalTestsContext
    {
        [Test]
        public void when_interview_is_created_event_received()
        {
            var interviewStatusesStorage = new TestInMemoryWriter<InterviewSummary>();
            var speedReportItemsStorage = new TestInMemoryWriter<SpeedReportInterviewItem>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewOnClientCreated(interviewId: Guid.NewGuid()));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: Guid.NewGuid()));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewFromPreloadedDataCreated(interviewId: Guid.NewGuid()));

            var denormalizer = CreateDenormalizer(interviewStatusesStorage, speedReportItemsStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent }, publishableEvent.EventSourceId);
            }

            Assert.True(statusEventsToPublish.TrueForAll(s => speedReportItemsStorage.GetById(s.EventSourceId.FormatGuid()) != null));
        }
    }
}
