using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    [TestFixture]
    internal class StatusChangeHistoryDenormalizerFunctionalTests : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [Test]
        public void when_create_interview_status_history_should_contains_timestaps_from_events()
        {
            var interviewId = Guid.NewGuid();
            var currentTime = DateTime.UtcNow;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId, createTime: currentTime.AddSeconds(1)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, assignTime: currentTime.AddSeconds(2)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, assignTime: currentTime.AddSeconds(3)));

            var statuses = interviewSummary.InterviewCommentedStatuses;
            Assert.AreEqual(statuses[0].Status, InterviewExportedAction.Created);
            Assert.AreEqual(statuses[0].Timestamp, currentTime.AddSeconds(1));
            Assert.AreEqual(statuses[1].Status, InterviewExportedAction.SupervisorAssigned);
            Assert.AreEqual(statuses[1].Timestamp, currentTime.AddSeconds(2));
            Assert.AreEqual(statuses[2].Status, InterviewExportedAction.InterviewerAssigned);
            Assert.AreEqual(statuses[2].Timestamp, currentTime.AddSeconds(3));
        }
    }
}