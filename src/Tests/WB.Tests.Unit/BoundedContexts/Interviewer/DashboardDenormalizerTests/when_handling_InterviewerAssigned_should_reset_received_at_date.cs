using System;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    public class when_handling_InterviewerAssigned
    {
        [Test]
        public void should_reset_received_at_date()
        {
            var dashboardItem = Create.Entity.InterviewView(interviewId: Id.g1);
            dashboardItem.ReceivedByInterviewerAtUtc = DateTime.UtcNow;

            var interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(dashboardItem);

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage);

            // Act 
            denormalizer.Handle(Create.PublishedEvent.InterviewerAssigned(interviewId: Id.g1));

            // Assert
            var changedItem = interviewViewStorage.GetById(Id.g1.FormatGuid());

            Assert.That(changedItem, Has.Property(nameof(changedItem.ReceivedByInterviewerAtUtc)).Null);
        }
    }
}
