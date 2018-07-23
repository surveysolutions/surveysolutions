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
    public class when_interview_was_rejected
    {
        [Test]
        public void should_not_be_possible_to_delete_it()
        {
            var rejectedByHqInterview = Id.g1;
            var rejectedBySvInterview = Id.g2;

            IPlainStorage<InterviewView> interviews = new InMemoryPlainStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(interviewId: rejectedByHqInterview, canBeDeleted: true));
            interviews.Store(Create.Entity.InterviewView(interviewId: rejectedBySvInterview, canBeDeleted: true));

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviews);

            var rejectedByHqEvent = Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, interviewId: rejectedByHqInterview);
            var rejectedBySvEvent = Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, interviewId: rejectedBySvInterview);

            // Act
            denormalizer.Handle(rejectedByHqEvent);
            denormalizer.Handle(rejectedBySvEvent);

            // Assert
            var rejectedByHqInterviewView = interviews.GetById(rejectedByHqInterview.FormatGuid());
            Assert.That(rejectedByHqInterviewView, Has.Property(nameof(rejectedByHqInterviewView.CanBeDeleted)).False);

            var rejectedBySvView = interviews.GetById(rejectedByHqInterview.FormatGuid());
            Assert.That(rejectedBySvView, Has.Property(nameof(rejectedBySvView.CanBeDeleted)).False);
        }
    }
}
