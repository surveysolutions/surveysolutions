using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
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

            var audioFilesStorage = new Mock<IAudioFileStorage>();
            var audioRecordingFile = Create.Entity.InterviewBinaryDataDescriptor();
            audioFilesStorage.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new List<InterviewBinaryDataDescriptor>
                {
                    audioRecordingFile
                });
            var multimediaStorage = new SqliteInmemoryStorage<InterviewMultimediaView>();
            multimediaStorage.Store(new InterviewMultimediaView
            {
                Id = Guid.NewGuid().FormatGuid(),
                InterviewId = interviewId,
                FileName = "multimedia1",
                FileId = "mult1"
            });

            var multimediaFiles = new InMemoryPlainStorage<InterviewFileView>();
            byte[] imageBytes = Array.Empty<byte>();
            multimediaFiles.Store(new InterviewFileView
            {
                File = imageBytes,
                Id = "mult1"
            });

            var syncronizer = CreateSynchronizer(interviewsDashboard, interviewAccessor.Object, syncClient.Object, 
                audioFileStorage: audioFilesStorage.Object,
                interviewMultimediaViewStorage: multimediaStorage,
                imagesStorage: multimediaFiles);
            UploadProgress reportedProgress = null;

            var endpoint = "foo";

            // Act
            await syncronizer.UploadPendingInterviews(endpoint, new Progress<UploadProgress>(o => reportedProgress = o), CancellationToken.None);

            // Assert
            interviewAccessor.Verify(x => x.RemoveInterview(interviewId));
            syncClient.Verify(x => x.PostInterviewAsync(endpoint, 
                It.Is<PostInterviewRequest>(r => r.Events.Equals(interviewEvents)), null));
            Assert.That(reportedProgress, Has.Property(nameof(reportedProgress.TotalToUpload)).EqualTo(1));
            Assert.That(reportedProgress, Has.Property(nameof(reportedProgress.UploadedCount)).EqualTo(1));

            syncClient.Verify(x => x.PostInterviewImageAsync(endpoint, It.Is<PostInterviewImageRequest>(r => 
                r.InterviewId == interviewId && r.Content.Equals(imageBytes) && r.FileName == "multimedia1"), null));

            syncClient.Verify(x => x.PostInterviewAudioAsync(endpoint, It.Is<PostInterviewAudioRequest>(r => 
                r.InterviewId == interviewId 
                && r.FileName == audioRecordingFile.FileName), null));
        }

        private InterviewsSynchronizer CreateSynchronizer(IPlainStorage<InterviewView> interviewStorage = null,
            IInterviewerInterviewAccessor interviewAccessor = null,
            IOfflineSyncClient syncClient = null,
            IAudioFileStorage audioFileStorage = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IPlainStorage<InterviewFileView> imagesStorage = null)
        {
            return new InterviewsSynchronizer(interviewStorage ?? new InMemoryPlainStorage<InterviewView>(),
                interviewAccessor ?? Mock.Of<IInterviewerInterviewAccessor>(),
                syncClient ?? Mock.Of<IOfflineSyncClient>(),
                audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                interviewMultimediaViewStorage ?? new InMemoryPlainStorage<InterviewMultimediaView>(),
                imagesStorage ?? new InMemoryPlainStorage<InterviewFileView>());
        }
    }
}
