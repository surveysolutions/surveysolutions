using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    [TestOf(typeof(InterviewDashboardEventHandler))]
    public class when_approved_by_supervisor
    {
        [Test]
        public void should_fill_approved_at_date()
        {
            var interviewId = Id.g1;

            IPlainStorage<InterviewView> interviews = new InMemoryPlainStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(interviewId: interviewId, canBeDeleted: true));

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviews);

            var approvalDate = DateTime.UtcNow;
            var statusChangeEvent = Create.Event.InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, originDate: approvalDate);

            // Act
            denormalizer.Handle(statusChangeEvent.ToPublishedEvent(eventSourceId: interviewId));

            // Assert
            var interview = interviews.GetById(interviewId.FormatGuid());
            Assert.That(interview, Has.Property(nameof(interview.ApprovedDateTimeUtc)).EqualTo(approvalDate));
        }
    }
}
