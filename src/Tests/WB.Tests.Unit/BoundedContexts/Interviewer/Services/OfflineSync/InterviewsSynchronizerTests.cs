using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.OfflineSync
{
    [TestOf(typeof(InterviewsSynchronizer))]
    public class InterviewsSynchronizerTests
    {
        [Test]
        public async Task should_upload_and_remove_completed_interview()
        {
            var interviewId = Id.gA;
            var interviewsDashboard = new InMemoryPlainStorage<InterviewView>();
            interviewsDashboard.Store(Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed));

            var interviewAccessor = new Mock<IInterviewerInterviewAccessor>();
            var interviewEvents = Enumerable.Empty<CommittedEvent>().ToReadOnlyCollection();
            interviewAccessor.Setup(x => x.GetPendingInteviewEvents(interviewId))
                .Returns(interviewEvents);

            var syncClient = new Mock<IOfflineSyncClient>();

            var syncronizer = CreateSynchronizer(interviewsDashboard, interviewAccessor.Object, syncClient.Object);
            UploadProgress reportedProgress = null;

            // Act
            await syncronizer.UploadPendingInterviews("foo", new Progress<UploadProgress>(o => reportedProgress = o));

            // Assert
            interviewAccessor.Verify(x => x.RemoveInterview(interviewId));
            syncClient.Verify(x => x.PostInterviewAsync("foo", 
                It.Is<PostInterviewRequest>(r => r.Events.Equals(interviewEvents)), null));
            Assert.That(reportedProgress, Has.Property(nameof(reportedProgress.TotalToUpload)).EqualTo(1));
            Assert.That(reportedProgress, Has.Property(nameof(reportedProgress.UploadedCount)).EqualTo(1));
        }

        private InterviewsSynchronizer CreateSynchronizer(IPlainStorage<InterviewView> interviewStorage = null,
            IInterviewerInterviewAccessor interviewAccessor = null,
            IOfflineSyncClient syncClient = null)
        {
            return new InterviewsSynchronizer(interviewStorage ?? new InMemoryPlainStorage<InterviewView>(),
                interviewAccessor ?? Mock.Of<IInterviewerInterviewAccessor>(),
                syncClient ?? Mock.Of<IOfflineSyncClient>());
        }
    }
}
